# C# Patterns for Unity Game Data

Project uses Unity (any modern LTS) with C# 9+. Data classes live under `Assets/Scripts/Data/`; loaders under `Assets/Scripts/Data/Loaders/`. Project-wide rules:

- **Files ≤ 450 lines.** Split by concern when approaching the limit.
- **Entries have `ulong` IDs** in `[WORD][WORD][DWORD]` format. See `id-encoding.md`.
- **One public type per file.** File name = type name.
- **No magic numbers in gameplay code** — tuning values come from the data registry.

## Naming conventions

- Files: `PascalCase.cs` matching the type name.
- Namespaces: `Game.Data`, `Game.Data.Loaders`, `Game.Gameplay`, …
- Types, properties, public methods: `PascalCase`.
- Fields, locals, parameters: `camelCase`.
- Constants: `PascalCase` (modern C# convention) — `public const ulong NoneId = 0UL;`.
- Enum types: `PascalCase`; enum members: `PascalCase`.

## Choosing a JSON library

Pick **one** per project and use it everywhere:

| Library                  | When to use                                                                |
| ------------------------ | -------------------------------------------------------------------------- |
| `UnityEngine.JsonUtility`| Smallest dependency. Limitations: no `Dictionary`, no polymorphism, fields must be public or `[SerializeField]`. Good enough for most flat data. |
| `Newtonsoft.Json` (com.unity.nuget.newtonsoft-json) | Full-featured. Use when you need dictionaries, polymorphism, or converters (e.g. `ulong` hex strings). |
| `System.Text.Json`       | Modern .NET. Works in Unity 2021+ with .NET Standard 2.1. Pick when team already standardizes on it. |

The examples below use **Newtonsoft**, because hex-string `ulong` and cross-reference resolution want a custom converter. If the project chose JsonUtility, the field shapes stay the same — only the parser changes.

## ContentId — helpers around a raw `ulong`

```csharp
// Assets/Scripts/Data/ContentId.cs
using System.Globalization;

namespace Game.Data
{
    public static class ContentId
    {
        public const ulong None = 0UL;

        public static ushort Category(ulong id)    => (ushort)((id >> 48) & 0xFFFF);
        public static ushort SubCategory(ulong id) => (ushort)((id >> 32) & 0xFFFF);
        public static uint   Index(ulong id)       => (uint)(id & 0xFFFFFFFF);

        public static ulong Compose(ushort category, ushort subCategory, uint index)
            => ((ulong)category << 48) | ((ulong)subCategory << 32) | index;

        public static ulong Parse(string hex)
        {
            var s = hex.Trim();
            if (s.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase)) s = s.Substring(2);
            return ulong.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        public static string ToHex(ulong id) => $"0x{id:X16}";
    }
}
```

## Enums

One enum per file when used as a domain type. The enum's auto-numbering must **not** drive ID encoding — IDs use the explicit hex constants in `id-encoding.md`.

```csharp
// Assets/Scripts/Data/ItemCategory.cs
namespace Game.Data
{
    public enum ItemCategory
    {
        None = 0,
        Mineral,
        Natural,
        Fluid,
        Intermediate,
        Facility,
        Storage,
        Transport,
        Weapon,
        SciencePack,
        Module,
    }
}
```

```csharp
// Assets/Scripts/Data/EnergyType.cs
namespace Game.Data
{
    public enum EnergyType { None = 0, Burner, Electric }
}
```

Parse strings to enums **once at load time** via `System.Enum.TryParse<T>(s, ignoreCase: true, out var v)`, then cache the enum value.

## Data classes — POCOs

Use plain C# records or classes with auto-properties. **Avoid `MonoBehaviour` and `ScriptableObject` for runtime-loaded JSON data** — they tie you to Unity's lifecycle and serializer rules.

```csharp
// Assets/Scripts/Data/ItemDef.cs
using Newtonsoft.Json;

namespace Game.Data
{
    public class ItemDef
    {
        [JsonConverter(typeof(HexUlongConverter))]
        public ulong Id { get; set; }

        public string Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ItemCategory Category { get; set; }

        public int StackSize { get; set; } = 100;
    }
}
```

```csharp
// Assets/Scripts/Data/EnergyDef.cs
namespace Game.Data
{
    public class EnergyDef
    {
        public EnergyType Type { get; set; }
        public float ConsumptionKw { get; set; }
        public float MinKw { get; set; }
        public float ProductionKw { get; set; }
    }
}
```

```csharp
// Assets/Scripts/Data/FacilityDef.cs
namespace Game.Data
{
    public class FacilityDef : ItemDef
    {
        public string SubCategory { get; set; }
        public int[] Size { get; set; }
        public int ModuleSlots { get; set; }
        public EnergyDef Energy { get; set; }
        public float PollutionPerMin { get; set; }
        public float SpeedPerSec { get; set; }
        public string BuildRecipe { get; set; }
    }
}
```

```csharp
// Assets/Scripts/Data/IngredientDef.cs
namespace Game.Data
{
    public class IngredientDef
    {
        public string Item { get; set; }
        public float Amount { get; set; }
    }
}
```

```csharp
// Assets/Scripts/Data/RecipeDef.cs
using System.Collections.Generic;

namespace Game.Data
{
    public class RecipeDef
    {
        public string Id { get; set; }
        public string[] ProducedIn { get; set; }
        public float TimeSec { get; set; }
        public List<IngredientDef> Ingredients { get; set; } = new();
        public List<IngredientDef> Results { get; set; } = new();
        public float EnergyPerCraftKj { get; set; }
    }
}
```

## HexUlongConverter

JSON stores IDs as `"0x...."` strings. Convert once at the boundary.

```csharp
// Assets/Scripts/Data/Loaders/HexUlongConverter.cs
using System;
using Newtonsoft.Json;

namespace Game.Data.Loaders
{
    public class HexUlongConverter : JsonConverter<ulong>
    {
        public override ulong ReadJson(JsonReader reader, Type t, ulong existing, bool hasExisting, JsonSerializer s)
            => ContentId.Parse((string)reader.Value);

        public override void WriteJson(JsonWriter writer, ulong value, JsonSerializer s)
            => writer.WriteValue(ContentId.ToHex(value));
    }
}
```

## DataRegistry — single entry point

Gameplay code asks `DataRegistry` for items and recipes. It never opens JSON itself. Initialize once at app boot and keep a process-wide instance (singleton or DI-registered).

```csharp
// Assets/Scripts/Data/Loaders/DataRegistry.cs
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data.Loaders
{
    public class DataRegistry
    {
        public static DataRegistry Instance { get; private set; }

        private readonly Dictionary<ulong, ItemDef> _byId = new();
        private readonly Dictionary<string, ItemDef> _byName = new();
        private readonly Dictionary<string, RecipeDef> _recipes = new();

        public IReadOnlyDictionary<ulong, ItemDef>  Items   => _byId;
        public IReadOnlyDictionary<string, RecipeDef> Recipes => _recipes;

        public static void Bootstrap()
        {
            Instance = new DataRegistry();
            Instance.LoadAll();
        }

        public void LoadAll()
        {
            ItemLoader.LoadFile(this,     "Data/items/minerals.json");
            ItemLoader.LoadFile(this,     "Data/items/intermediates.json");
            FacilityLoader.LoadFile(this, "Data/facilities/mining.json");
            RecipeLoader.LoadFile(this,   "Data/recipes/smelting.json");
            Validate();
        }

        public ItemDef GetItem(ulong id)            => _byId[id];
        public ItemDef GetItemByName(string name)   => _byName[name];
        public RecipeDef GetRecipe(string id)       => _recipes[id];

        public void RegisterItem(ItemDef def)
        {
            if (_byId.ContainsKey(def.Id))
            {
                Debug.LogError($"Duplicate ContentId {ContentId.ToHex(def.Id)} for '{def.Name}'");
                return;
            }
            if (_byName.ContainsKey(def.Name))
            {
                Debug.LogError($"Duplicate item name '{def.Name}'");
                return;
            }
            _byId[def.Id] = def;
            _byName[def.Name] = def;
        }

        public void RegisterRecipe(RecipeDef recipe)
        {
            if (_recipes.ContainsKey(recipe.Id))
            {
                Debug.LogError($"Duplicate recipe id '{recipe.Id}'");
                return;
            }
            _recipes[recipe.Id] = recipe;
        }

        private void Validate()
        {
            foreach (var r in _recipes.Values)
            {
                foreach (var ing in r.Ingredients)
                    if (!_byName.ContainsKey(ing.Item))
                        Debug.LogError($"Recipe '{r.Id}' references unknown ingredient '{ing.Item}'");
                foreach (var res in r.Results)
                    if (!_byName.ContainsKey(res.Item))
                        Debug.LogError($"Recipe '{r.Id}' references unknown result '{res.Item}'");
                foreach (var fac in r.ProducedIn)
                    if (!_byName.ContainsKey(fac))
                        Debug.LogError($"Recipe '{r.Id}' references unknown facility '{fac}'");
            }
        }
    }
}
```

If `DataRegistry` approaches 450 lines, split off `Validators/ReferenceValidator.cs` first — orchestration stays in the registry.

## JsonLoader — abstracts the platform difference

`StreamingAssets` lives on disk on standalone platforms but inside the APK/jar on Android, and behind HTTP on WebGL. The loader hides this.

```csharp
// Assets/Scripts/Data/Loaders/JsonLoader.cs
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Data.Loaders
{
    public static class JsonLoader
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static T Load<T>(string relativePath)
        {
            var text = ReadAllText(relativePath);
            if (text == null) return default;
            try
            {
                return JsonConvert.DeserializeObject<T>(text, Settings);
            }
            catch (JsonException e)
            {
                Debug.LogError($"JSON parse error in {relativePath}: {e.Message}");
                return default;
            }
        }

        private static string ReadAllText(string relativePath)
        {
            var path = Path.Combine(Application.streamingAssetsPath, relativePath);
#if UNITY_ANDROID && !UNITY_EDITOR
            using var req = UnityEngine.Networking.UnityWebRequest.Get(path);
            req.SendWebRequest();
            while (!req.isDone) {}
            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Cannot read {path}: {req.error}");
                return null;
            }
            return req.downloadHandler.text;
#else
            if (!File.Exists(path))
            {
                Debug.LogError($"Missing data file: {path}");
                return null;
            }
            return File.ReadAllText(path);
#endif
        }
    }
}
```

For WebGL or async loading, replace the blocking Android branch with a coroutine/UniTask variant.

## Per-category loaders

Each loader unwraps the envelope and registers entries.

```csharp
// Assets/Scripts/Data/Loaders/ItemLoader.cs
using System.Collections.Generic;

namespace Game.Data.Loaders
{
    public static class ItemLoader
    {
        private class Envelope { public List<ItemDef> Items { get; set; } }

        public static void LoadFile(DataRegistry registry, string relativePath)
        {
            var env = JsonLoader.Load<Envelope>(relativePath);
            if (env?.Items == null) return;
            foreach (var def in env.Items) registry.RegisterItem(def);
        }
    }
}
```

The recipe and facility loaders mirror the same shape with their own envelope class.

## Bootstrap

Call `DataRegistry.Bootstrap()` once during app startup — for example from a `Bootstrap` `MonoBehaviour` in your first scene:

```csharp
using UnityEngine;
using Game.Data.Loaders;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        DataRegistry.Bootstrap();
    }
}
```

Then `DataRegistry.Instance.GetItemByName("IronPlate")` (load-time only) or `DataRegistry.Instance.GetItem(id)` (runtime) from anywhere.

## ScriptableObject variant

If the project chose ScriptableObjects instead of JSON, the data classes become:

```csharp
[CreateAssetMenu(menuName = "Data/Item")]
public class ItemDefSO : ScriptableObject
{
    public ulong Id;
    public string DisplayName;
    public ItemCategory Category;
    public int StackSize = 100;
}
```

…and the loader uses `Resources.LoadAll<ItemDefSO>("Data/Items")` or Addressables instead of `JsonLoader`. The rest of this skill — IDs, categories, registry, validation — is unchanged.

## File-size discipline

If `DataRegistry` orchestration alone exceeds 450 lines, that's a smell — the orchestration list shouldn't be that long. More likely the validator grows; split validators into `Validators/ReferenceValidator.cs` when that happens.
