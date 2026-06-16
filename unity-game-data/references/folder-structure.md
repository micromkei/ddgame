# Recommended Unity Project Layout

Two valid data-storage strategies in Unity:

- **JSON under `StreamingAssets/`** — best for large tabular data, hot-reload, content patches, and external editors. Recommended default.
- **ScriptableObject assets under `Assets/Data/`** — best for small curated content edited inside the Unity Inspector and referenced by drag-and-drop.

Pick **one** as the source of truth per content kind. Mixing them for the same kind creates ambiguity about which wins on conflict. JSON is the default below; switch to SO only when the editor workflow clearly pays off.

## Canonical layout (JSON-first)

```
Assets/
├── StreamingAssets/
│   └── Data/                          # JSON data — mirrors the GDD
│       ├── items/
│       │   ├── minerals.json
│       │   ├── intermediates.json
│       │   └── ...                    # split by subcategory
│       ├── facilities/
│       │   └── ...
│       ├── recipes/
│       │   └── ...
│       └── manifest.json              # optional: list of files to load
├── Data/                              # ScriptableObject assets (only if used)
│   └── ...
├── Scripts/
│   ├── Data/                          # C# data classes (one type per file)
│   │   ├── ContentId.cs
│   │   ├── ItemCategory.cs
│   │   ├── ItemDef.cs
│   │   ├── FacilityDef.cs
│   │   ├── RecipeDef.cs
│   │   └── ...
│   ├── Data/Loaders/
│   │   ├── DataRegistry.cs            # singleton entry point
│   │   ├── JsonLoader.cs
│   │   ├── ItemLoader.cs
│   │   ├── FacilityLoader.cs
│   │   └── RecipeLoader.cs
│   ├── Data/Validators/               # split off DataRegistry when it grows
│   │   └── ReferenceValidator.cs
│   └── Gameplay/                      # systems that consume data
└── ...

Reference/
└── GameDesignDocument.md              # GDD — source of truth, do not auto-edit
```

`Assets/StreamingAssets/` is shipped verbatim with the build and read via `Application.streamingAssetsPath`. On Android/WebGL it lives inside the APK/bundle — read it with `UnityWebRequest` instead of `File.ReadAllText`. The `JsonLoader` should hide that platform difference.

## Why split this way?

- **Small files.** Each JSON stays well below the 450-line cap.
- **Diffability.** Editing one subcategory doesn't churn unrelated files.
- **Loader simplicity.** The registry iterates a fixed list of `(path, type)` pairs, or reads `manifest.json`.
- **Matches GDD sections.** Folder boundaries follow the GDD's table-of-contents.

## When to add a new file vs. extend existing

Add a new file when:
- A new subcategory appears.
- An existing JSON approaches ~300 lines.

Extend the existing file when:
- Adding the next tier in a family already in the file.
- Adding individual rows to an existing category list.

## Naming

- Files: `snake_case.json` for data, `PascalCase.cs` for C# (Unity/.NET convention).
- C# types: `PascalCase`, one public type per file, file name = type name.
- Members: `PascalCase` for properties, `camelCase` for locals and serialized fields.
- Content names inside JSON: stable PascalCase identifiers from the GDD (`IronPlate`, `ElectricMiningDrill`), used as cross-reference keys. Display names go in a separate `displayName` field if they differ.

## Optional: `manifest.json`

If hand-maintaining the list of data files in `DataRegistry` grows tedious, drop a `manifest.json` next to the folders:

```json
{
  "items":      ["items/minerals.json", "items/intermediates.json"],
  "facilities": ["facilities/mining.json", "facilities/furnaces.json"],
  "recipes":    ["recipes/smelting.json", "recipes/crafting.json"]
}
```

The registry reads the manifest first, then iterates. New data files become a one-line manifest edit instead of a C# change.

## ScriptableObject variant (only if chosen)

If a project uses SOs instead of JSON:

```
Assets/
├── Data/
│   ├── Items/
│   │   ├── IronPlate.asset
│   │   └── ...
│   └── Recipes/
│       └── ...
├── Scripts/
│   └── Data/
│       ├── ItemDefSO.cs       # ScriptableObject subclass with [CreateAssetMenu]
│       └── ...
```

The skill still applies — schemas, IDs, validation, and the registry pattern carry over. The only differences: data classes inherit `ScriptableObject` (not POCO), and the loader uses `Resources.LoadAll<T>` or `AssetDatabase` (Editor only) / `Addressables` (runtime) instead of `JsonLoader`.
