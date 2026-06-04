# JSON Data Schemas

Every data file is a JSON object with a single top-level array. The wrapping field name matches the data kind:

```json
{ "items": [ ... ] }
```

```json
{ "facilities": [ ... ] }
```

```json
{ "recipes": [ ... ] }
```

Never write a naked array as the root; it makes future schema additions painful.

## ItemDef — base item

Used in everything under `data/items/`, and as the parent shape for facilities, storage, transport, modules, weapons, and science packs.

```json
{
  "id": "0x0004000000000005",
  "name": "IronPlate",
  "category": "Intermediate",
  "stack_size": 100
}
```

| Field      | Type    | Required | Notes                                                     |
| ---------- | ------- | -------- | --------------------------------------------------------- |
| id         | string  | yes      | Hex uint64. See `id-encoding.md`.                         |
| name       | string  | yes      | English name from the GDD. Used as the recipe lookup key. |
| category   | string  | yes      | One of the `ItemCategory` enum names.                     |
| stack_size | integer | no       | Default 100 if not specified.                             |

## FacilityDef — extends ItemDef

Used in `data/facilities/`. Adds the physical-/gameplay-relevant fields the GDD specifies.

```json
{
  "id": "0x0005000100000002",
  "name": "ElectricMiningDrill",
  "category": "Facility",
  "subcategory": "MiningDrill",
  "size": [5, 5],
  "module_slots": 3,
  "energy": { "type": "Electric", "consumption_kw": 90 },
  "pollution_per_min": 10,
  "speed_per_sec": 0.5,
  "build_recipe": "ElectricMiningDrill"
}
```

| Field             | Type    | Required | Notes                                                |
| ----------------- | ------- | -------- | ---------------------------------------------------- |
| subcategory       | string  | yes      | Must match `id-encoding.md` MID WORD names.          |
| size              | int[2]  | yes      | `[width, height]` in tiles.                          |
| module_slots      | integer | no       | Omit if 0.                                           |
| energy            | object  | yes      | See EnergyDef below.                                 |
| pollution_per_min | number  | no       | Omit if 0 (e.g., CoastalPump).                       |
| speed_per_sec     | number  | depends  | Mining drills, assembly machines, belts.             |
| build_recipe      | string  | yes      | Recipe id that crafts this facility.                 |

## EnergyDef

```json
{ "type": "Electric", "consumption_kw": 90 }
```

| Field          | Type   | Required | Notes                              |
| -------------- | ------ | -------- | ---------------------------------- |
| type           | string | yes      | `"Burner"` or `"Electric"`.        |
| consumption_kw | number | yes      | Peak draw in kW.                   |
| min_kw         | number | no       | For machines with idle draw range. |

For **power generators** (Boiler, SteamEngine), use:
```json
{ "type": "Electric", "production_kw": 900 }
```

## TransportDef — extends ItemDef

Used in `data/transport/`. Belts, undergrounds, distributors.

```json
{
  "id": "0x0007000100000004",
  "name": "FastConveyorBelt",
  "category": "Transport",
  "subcategory": "Belt",
  "speed_items_per_sec": 30,
  "underground_length": 7,
  "build_recipe": "FastConveyorBelt"
}
```

`underground_length` is meaningful only for underground belts.

## StorageDef — extends ItemDef

```json
{
  "id": "0x0006000100000001",
  "name": "WoodBox",
  "category": "StorageBox",
  "subcategory": "Box",
  "slots": 16,
  "hp": 100,
  "build_recipe": "WoodBox"
}
```

## RecipeDef

Used in `data/recipes/`.

```json
{
  "id": "IronPlate",
  "produced_in": ["StoneFurnace", "IronFurnace", "ElectricFurnace"],
  "time_sec": 3.2,
  "ingredients": [
    { "item": "IronOre", "amount": 1 }
  ],
  "results": [
    { "item": "IronPlate", "amount": 1 }
  ],
  "energy_per_craft_kj": 288
}
```

| Field               | Type      | Required | Notes                                                          |
| ------------------- | --------- | -------- | -------------------------------------------------------------- |
| id                  | string    | yes      | Recipe key, usually the primary result name. Globally unique.  |
| produced_in         | string[]  | yes      | Names of facilities that can craft this recipe.                |
| time_sec            | number    | yes      | Base craft time at facility speed = 1.                         |
| ingredients         | object[]  | yes      | Each `{ item, amount }`. May be empty (e.g., extraction).      |
| results             | object[]  | yes      | At least one entry.                                            |
| energy_per_craft_kj | number    | no       | Furnace recipes only — use the StoneFurnace baseline from GDD. |

For recipes producing multiple outputs (e.g., HighGrade Crude Oil Process), list each in `results`.

For fluid recipes, `amount` is the fluid quantity from the GDD as-is — the unit context comes from the item's `category: "Fluid"`.

## Common rules

- **Numbers use the GDD's units.** `consumption_kw` not watts. `time_sec` not ms. `pollution_per_min` not per-tick. `speed_per_sec` for production / `speed_items_per_sec` for belts.
- **Names match the GDD exactly,** PascalCase (`IronPlate`, not `Iron Plate` or `iron_plate`).
- **Ingredient/result items must resolve.** The validator should reject unknown names.
- **No nulls.** Omit optional fields rather than serializing `null`.
- **Numbers are numbers, not strings.** Don't quote `90` as `"90"`.
