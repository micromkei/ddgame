# GDScript Patterns for Game Data

Project uses Godot 4.x with GDScript. Data classes live under `scripts/data/`; loaders under `scripts/loaders/`. Project-wide rules (from `.claude/CLAUDE.local.md`):

- **Files ≤ 450 lines.** Split by concern when approaching the limit.
- **Items have `int` IDs** in `[WORD][WORD][DWORD]` format. Godot's `int` is signed 64-bit, which fits the encoding.

## Naming conventions

- Files: `snake_case.gd`
- `class_name`: `PascalCase` (one type per file, file name matches the class name in snake_case)
- Variables, functions, members: `snake_case`
- Constants: `SCREAMING_SNAKE_CASE`
- Enum types: `PascalCase`; enum members: `SCREAMING_SNAKE_CASE`

## ItemId — helpers around a raw `int`

GDScript has no value types or generics. Treat IDs as plain `int` values everywhere and route bit-level work through a static helper class. Don't pass dictionaries pretending to be IDs.

```gdscript
# scripts/data/item_id.gd
class_name ItemId
extends RefCounted

static func category(id: int) -> int:
    return (id >> 48) & 0xFFFF

static func sub_category(id: int) -> int:
    return (id >> 32) & 0xFFFF

static func index(id: int) -> int:
    return id & 0xFFFFFFFF

static func parse(hex: String) -> int:
    var clean := hex.strip_edges()
    if clean.to_lower().begins_with("0x"):
        clean = clean.substr(2)
    return ("0x" + clean).hex_to_int()

static func to_hex(id: int) -> String:
    return "0x%016X" % id
```

## Enums

GDScript enums live inside a class. Keep one enum per file when used as a domain type, so `class_name` references stay clean.

```gdscript
# scripts/data/item_category.gd
class_name ItemCategory

enum Type {
    MINERAL,
    NATURAL,
    FLUID,
    INTERMEDIATE,
    FACILITY,
    STORAGE_BOX,
    TRANSPORT,
    WEAPON,
    SCIENCE_PACK,
    MODULE,
}

static func from_string(s: String) -> Type:
    return Type[s.to_upper()]
```

```gdscript
# scripts/data/energy_type.gd
class_name EnergyType

enum Type { BURNER, ELECTRIC }

static func from_string(s: String) -> Type:
    return Type[s.to_upper()]
```

The enum's auto-numbering must **not** drive ID encoding — IDs use the explicit hex constants in `id-encoding.md`. The enum is for `match`-style logic only.

## Data classes

GDScript has no `record` type. Use lightweight `RefCounted` classes with typed fields and a `from_dict` static factory. One type per file.

```gdscript
# scripts/data/item_def.gd
class_name ItemDef
extends RefCounted

var id: int
var name: String
var category: ItemCategory.Type
var stack_size: int = 100

static func from_dict(d: Dictionary) -> ItemDef:
    var def := ItemDef.new()
    def.id = ItemId.parse(d["id"])
    def.name = d["name"]
    def.category = ItemCategory.from_string(d["category"])
    def.stack_size = int(d.get("stack_size", 100))
    return def
```

```gdscript
# scripts/data/energy_def.gd
class_name EnergyDef
extends RefCounted

var type: EnergyType.Type
var consumption_kw: float
var min_kw: float = 0.0          # 0.0 means "unset"
var production_kw: float = 0.0   # 0.0 means "unset"

static func from_dict(d: Dictionary) -> EnergyDef:
    var e := EnergyDef.new()
    e.type = EnergyType.from_string(d["type"])
    e.consumption_kw = float(d.get("consumption_kw", 0.0))
    e.min_kw = float(d.get("min_kw", 0.0))
    e.production_kw = float(d.get("production_kw", 0.0))
    return e
```

```gdscript
# scripts/data/facility_def.gd
class_name FacilityDef
extends ItemDef

var sub_category: String
var size: PackedInt32Array
var module_slots: int
var energy: EnergyDef
var pollution_per_min: float
var speed_per_sec: float
var build_recipe: String

static func from_facility_dict(d: Dictionary) -> FacilityDef:
    var f := FacilityDef.new()
    f.id = ItemId.parse(d["id"])
    f.name = d["name"]
    f.category = ItemCategory.from_string(d["category"])
    f.stack_size = int(d.get("stack_size", 100))
    f.sub_category = d["sub_category"]
    f.size = PackedInt32Array(d["size"])
    f.module_slots = int(d["module_slots"])
    f.energy = EnergyDef.from_dict(d["energy"])
    f.pollution_per_min = float(d["pollution_per_min"])
    f.speed_per_sec = float(d["speed_per_sec"])
    f.build_recipe = d["build_recipe"]
    return f
```

```gdscript
# scripts/data/ingredient_def.gd
class_name IngredientDef
extends RefCounted

var item: String
var amount: float

static func from_dict(d: Dictionary) -> IngredientDef:
    var i := IngredientDef.new()
    i.item = d["item"]
    i.amount = float(d["amount"])
    return i
```

```gdscript
# scripts/data/recipe_def.gd
class_name RecipeDef
extends RefCounted

var id: String
var produced_in: PackedStringArray
var time_sec: float
var ingredients: Array[IngredientDef] = []
var results: Array[IngredientDef] = []
var energy_per_craft_kj: float = 0.0  # 0.0 means "unset"

static func from_dict(d: Dictionary) -> RecipeDef:
    var r := RecipeDef.new()
    r.id = d["id"]
    r.produced_in = PackedStringArray(d["produced_in"])
    r.time_sec = float(d["time_sec"])
    for ing in d["ingredients"]:
        r.ingredients.append(IngredientDef.from_dict(ing))
    for res in d["results"]:
        r.results.append(IngredientDef.from_dict(res))
    r.energy_per_craft_kj = float(d.get("energy_per_craft_kj", 0.0))
    return r
```

## GameDataLoader — single entry point

Gameplay code asks `GameDataLoader` for items and recipes. It never opens JSON itself. Register it as an autoload (e.g. `GameData`) so all systems share one instance.

```gdscript
# scripts/loaders/game_data_loader.gd
class_name GameDataLoader
extends RefCounted

var _by_id: Dictionary = {}    # int -> ItemDef
var _by_name: Dictionary = {}  # String -> ItemDef
var _recipes: Dictionary = {}  # String -> RecipeDef

func items() -> Dictionary: return _by_id
func recipes() -> Dictionary: return _recipes

func load_all() -> void:
    ItemLoader.load_file(self, "res://data/items/minerals.json")
    ItemLoader.load_file(self, "res://data/items/naturals.json")
    # ... one line per file in res://data/items/
    FacilityLoader.load_file(self, "res://data/facilities/mining.json")
    # ... one line per file in res://data/facilities/
    RecipeLoader.load_file(self, "res://data/recipes/smelting.json")
    # ... one line per file in res://data/recipes/
    _validate()

func get_item(id: int) -> ItemDef:
    return _by_id[id]

func get_item_by_name(name: String) -> ItemDef:
    return _by_name[name]

func get_recipe(id: String) -> RecipeDef:
    return _recipes[id]

func register_item(def: ItemDef) -> void:
    if _by_id.has(def.id):
        push_error("Duplicate ItemId %s for '%s'" % [ItemId.to_hex(def.id), def.name])
        return
    if _by_name.has(def.name):
        push_error("Duplicate item name '%s'" % def.name)
        return
    _by_id[def.id] = def
    _by_name[def.name] = def

func register_recipe(recipe: RecipeDef) -> void:
    if _recipes.has(recipe.id):
        push_error("Duplicate recipe id '%s'" % recipe.id)
        return
    _recipes[recipe.id] = recipe

func _validate() -> void:
    for r: RecipeDef in _recipes.values():
        for ing: IngredientDef in r.ingredients:
            if not _by_name.has(ing.item):
                push_error("Recipe '%s' references unknown ingredient '%s'" % [r.id, ing.item])
        for res: IngredientDef in r.results:
            if not _by_name.has(res.item):
                push_error("Recipe '%s' references unknown result '%s'" % [r.id, res.item])
        for fac in r.produced_in:
            if not _by_name.has(fac):
                push_error("Recipe '%s' references unknown facility '%s'" % [r.id, fac])
```

If `game_data_loader.gd` approaches 450 lines, the per-category loaders (`ItemLoader`, `FacilityLoader`, `RecipeLoader`) absorb the growth — `GameDataLoader` itself stays as orchestration.

## JSON loading from `res://`

GDScript's built-in `JSON` parses to `Dictionary` / `Array`. Wrap it once and route every loader through the wrapper.

```gdscript
# scripts/loaders/json_loader.gd
class_name JsonLoader

static func load_file(res_path: String) -> Variant:
    var file := FileAccess.open(res_path, FileAccess.READ)
    if file == null:
        push_error("Cannot open %s (err=%d)" % [res_path, FileAccess.get_open_error()])
        return null
    var text := file.get_as_text()
    var json := JSON.new()
    var err := json.parse(text)
    if err != OK:
        push_error("JSON parse error in %s at line %d: %s" % [
            res_path, json.get_error_line(), json.get_error_message()
        ])
        return null
    return json.data
```

Per-category loader pattern:

```gdscript
# scripts/loaders/item_loader.gd
class_name ItemLoader

static func load_file(loader: GameDataLoader, res_path: String) -> void:
    var data: Variant = JsonLoader.load_file(res_path)
    if not data is Array:
        push_error("Expected array at %s" % res_path)
        return
    for entry in data:
        loader.register_item(ItemDef.from_dict(entry))
```

JSON keys stay `snake_case` to match the GDD's units and field names (`stack_size`, `consumption_kw`, `pollution_per_min`). The `from_dict` factories read those keys directly — no automatic naming policy.

## Autoload registration

Make the loader globally available so gameplay code never re-opens JSON:

```
# project.godot (Autoload section)
GameData="*res://scripts/loaders/game_data_loader.gd"
```

Then call `GameData.load_all()` once during boot (e.g. from a `Main` scene's `_ready`), and use `GameData.get_item_by_name("IronPlate")` from anywhere.

## File-size discipline

If `GameDataLoader` orchestration alone exceeds 450 lines, that's a smell — the orchestration list shouldn't be that long. More likely it's the validator that grows; split validators into their own file (`scripts/loaders/validators.gd`) when that happens.
