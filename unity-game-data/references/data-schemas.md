# JSON Data Schemas

This file describes the **shape conventions** for game-data JSON. Specific field lists for items/recipes/etc. are illustrative — adopt them as a starting point, then extend per project. The conventions below stay constant.

## Envelope

Every data file is a JSON object with a single top-level array, named after the data kind:

```json
{ "items":      [ ... ] }
```

```json
{ "facilities": [ ... ] }
```

```json
{ "recipes":    [ ... ] }
```

Never write a naked array as the root — it makes future schema additions painful (e.g. a `schemaVersion` header).

Optional file-level fields the loader may consume:

```json
{
  "schemaVersion": 1,
  "items": [ ... ]
}
```

## Base shape — ContentDef

Every entry, regardless of kind, has these three required fields:

| Field    | Type   | Required | Notes                                                  |
| -------- | ------ | -------- | ------------------------------------------------------ |
| id       | string | yes      | Hex `ulong`. See `id-encoding.md`.                     |
| name     | string | yes      | Stable PascalCase identifier; used as cross-ref key.   |
| category | string | yes      | One of the project's `ItemCategory` enum names.        |

Optional but common:

| Field         | Type    | Notes                                                  |
| ------------- | ------- | ------------------------------------------------------ |
| displayName   | string  | Localized/UI-facing string; defaults to `name`.        |
| description   | string  | Tooltip / encyclopedia body.                           |
| icon          | string  | Resource path or Addressable key, not an embedded URL. |

## Example: ItemDef

A typical inventory item — extends the base shape with stack size.

```json
{
  "id": "0x0004000000000005",
  "name": "IronPlate",
  "category": "Intermediate",
  "stackSize": 100
}
```

| Field     | Type    | Required | Notes                                |
| --------- | ------- | -------- | ------------------------------------ |
| stackSize | integer | no       | Default 100 if not specified.        |

## Example: FacilityDef (extends ItemDef)

Adds gameplay-relevant fields for a placeable machine.

```json
{
  "id": "0x0005000100000002",
  "name": "ElectricMiningDrill",
  "category": "Facility",
  "subCategory": "MiningDrill",
  "size": [5, 5],
  "moduleSlots": 3,
  "energy": { "type": "Electric", "consumptionKw": 90 },
  "pollutionPerMin": 10,
  "speedPerSec": 0.5,
  "buildRecipe": "ElectricMiningDrill"
}
```

| Field           | Type    | Required | Notes                                       |
| --------------- | ------- | -------- | ------------------------------------------- |
| subCategory     | string  | yes      | Must match `id-encoding.md` MID WORD names. |
| size            | int[2]  | yes      | `[width, height]` in tiles.                 |
| moduleSlots     | integer | no       | Omit if 0.                                  |
| energy          | object  | yes      | See EnergyDef below.                        |
| pollutionPerMin | number  | no       | Omit if 0.                                  |
| speedPerSec     | number  | depends  | Required for production facilities.         |
| buildRecipe     | string  | yes      | Recipe id that crafts this facility.        |

## Example: EnergyDef

```json
{ "type": "Electric", "consumptionKw": 90 }
```

| Field         | Type   | Required | Notes                              |
| ------------- | ------ | -------- | ---------------------------------- |
| type          | string | yes      | `"Burner"` or `"Electric"`, etc.   |
| consumptionKw | number | yes      | Peak draw in kW.                   |
| minKw         | number | no       | For machines with idle draw range. |

For power generators, swap to `productionKw`:
```json
{ "type": "Electric", "productionKw": 900 }
```

## Example: RecipeDef

```json
{
  "id": "IronPlate",
  "producedIn": ["StoneFurnace", "IronFurnace", "ElectricFurnace"],
  "timeSec": 3.2,
  "ingredients": [
    { "item": "IronOre", "amount": 1 }
  ],
  "results": [
    { "item": "IronPlate", "amount": 1 }
  ],
  "energyPerCraftKj": 288
}
```

| Field            | Type     | Required | Notes                                                          |
| ---------------- | -------- | -------- | -------------------------------------------------------------- |
| id               | string   | yes      | Recipe key, usually the primary result name. Globally unique.  |
| producedIn       | string[] | yes      | Names of facilities that can craft this recipe.                |
| timeSec          | number   | yes      | Base craft time at facility speed = 1.                         |
| ingredients      | object[] | yes      | Each `{ item, amount }`. May be empty.                         |
| results          | object[] | yes      | At least one entry.                                            |
| energyPerCraftKj | number   | no       | Only when recipe imposes its own energy cost.                  |

## Common rules

- **Numbers use the GDD's units, in the field name.** `consumptionKw`, not `consumption`. `timeSec`, not `time`. `pollutionPerMin`, not `pollutionRate`. The unit is impossible to misread.
- **Identifiers (`name`, recipe `id`) match the GDD exactly,** PascalCase. No spaces, no snake_case.
- **Field names** follow Unity/C# convention: `camelCase` in JSON, `PascalCase` properties in C#. Pick one JSON casing and stick with it project-wide — `JsonUtility` is case-sensitive.
- **Cross-references must resolve.** The loader's validator rejects unknown names.
- **No nulls.** Omit optional fields rather than serializing `null`.
- **Numbers are numbers, not strings.** Don't quote `90` as `"90"`. (Exception: `ulong` IDs are hex strings because JSON can't safely represent them.)

## Extending the schema

When adding a new content kind (enemies, abilities, dialogue lines, loot tables):

1. **Define the shape here first** with required/optional fields and units.
2. **Pick a category code** in `id-encoding.md`.
3. **Add a C# data class** in `Assets/Scripts/Data/` (see `csharp-patterns.md`).
4. **Register a loader** in `DataRegistry` for the new file(s).
5. **Add validators** for any cross-references the new kind introduces.
