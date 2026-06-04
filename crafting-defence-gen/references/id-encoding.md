# Item ID Encoding

Every item, facility, module, weapon, science pack, and recipe input/output is keyed by a `uint64` ID. The ID is **hierarchical** — its high bits encode the category, so the runtime can decode an ID back into a category without a separate lookup.

The format comes from `.claude/CLAUDE.local.md`:

> 모든 item 및 자원은 unsigned int64 형태의 id key 를 갖는다. id key 는 [WORD][WORD][DWORD] 형태로 Category 정보를 포함하여 계층적으로 만들어 unique 하게 부여 되도록 한다.

## Layout

```
| HIGH WORD (16-bit) | MID WORD (16-bit) |     DWORD (32-bit)      |
|     Category       |  SubCategory/Tier |   Sequential index      |
```

Total: 64 bits. GDScript's `int` is signed 64-bit — there is no unsigned type — so every ID is stored as a (non-negative) `int` as long as the HIGH WORD stays below `0x8000` (see "Sign-bit headroom" below).

In hex notation: `0xCCCC_SSSS_IIIIIIII`.

## HIGH WORD — Category

Use stable hex constants. Do **not** rely on the GDScript `ItemCategory.Type` enum's auto-numbering — if the enum is reordered later, IDs would silently shift.

| Category       | Code   |
| -------------- | ------ |
| Mineral        | 0x0001 |
| Natural        | 0x0002 |
| Fluid          | 0x0003 |
| Intermediate   | 0x0004 |
| Facility       | 0x0005 |
| StorageBox     | 0x0006 |
| Transport      | 0x0007 |
| Weapon         | 0x0008 |
| SciencePack    | 0x0009 |
| Module         | 0x000A |

**Sign-bit headroom.** New category codes MUST stay below `0x8000`. The full uint64 range from the GDD spec is conceptual — at runtime IDs live in GDScript's signed `int`, and any code in `[0x8000, 0xFFFF]` would push the resulting ID into the negative half (it still round-trips through bit ops, but breaks `%016X` formatting and human-readable comparison). Current usage runs `0x0001..0x000A`, leaving 32k+ slots before this becomes a concern.

## MID WORD — SubCategory

Use `0x0000` if the category has no natural subdivision (Mineral, Natural, Fluid, Intermediate, SciencePack, Module).

For categories that do subdivide:

**Facility**
| SubCategory      | Code   |
| ---------------- | ------ |
| MiningDrill      | 0x0001 |
| Furnace          | 0x0002 |
| AssemblyMachine  | 0x0003 |
| Power            | 0x0004 |  (Boiler, SteamEngine)
| Pump             | 0x0005 |  (CoastalPump, DrillingRig, Feeder)

**Transport**
| SubCategory   | Code   |
| ------------- | ------ |
| Belt          | 0x0001 |  (Conveyor / Underground / Distributor — all tiers)
| Pipe          | 0x0002 |
| Pole          | 0x0003 |

**StorageBox**
| SubCategory   | Code   |
| ------------- | ------ |
| Box           | 0x0001 |

**Weapon**
| SubCategory   | Code   |
| ------------- | ------ |
| Ammo          | 0x0001 |  (FlamethrowerAmmo, CannonShell, ...)
| Capsule       | 0x0002 |  (PoisonCapsule, DecelerationCapsule, ...)
| Firearm       | 0x0003 |  (Shotgun, CombatShotgun)
| Explosive     | 0x0004 |  (Grenade, CliffBomb)
| Defense       | 0x0005 |  (Radar, FlyingRobotFrame)

If a new subcategory is needed, add it to this file before assigning IDs.

## DWORD — Sequential index

Starts at `0x00000001` within each `(Category, SubCategory)` pair. Append-only. Never reuse an index after deletion — leave gaps.

`0x00000000` is reserved for "none / unset". Do not assign it.

## JSON representation

Write IDs as 16-character hex strings with the `0x` prefix and no underscores:

```json
{ "id": "0x0004000000000005", "name": "IronPlate" }
```

GDScript parses these via `ItemId.parse` (see `gdscript-patterns.md`).

## Examples

`IronPlate` (Intermediate, no subcategory, 5th item):
```
0x0004_0000_0000_0005   →   "0x0004000000000005"
```

`ElectricMiningDrill` (Facility / MiningDrill / 2nd):
```
0x0005_0001_0000_0002   →   "0x0005000100000002"
```

`FastConveyorBelt` (Transport / Belt / index — pick next free):
```
0x0007_0001_0000_000?   →   look up the highest current Belt index, add 1
```

## Rules

1. **Append-only.** Once an ID is assigned and committed, never repurpose it — gameplay code, save files, and any deployed builds reference it.
2. **Validate uniqueness across all JSON files.** Before assigning a new ID, grep all `data/**/*.json` for it.
3. **Decode helpers belong in code, not in data.** Don't store `category` redundantly when it's derivable from the ID — but for human-readable JSON, including the `category` name field is fine and serves as a sanity check.
4. **Mismatch = bug.** If `id`'s high word disagrees with the `category` field, that's a validation error and the loader should reject it.
5. all id uint key, no string