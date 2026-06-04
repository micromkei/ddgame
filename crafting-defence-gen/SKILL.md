---
name: crafting-defence-gen
description: Manage Factory Defence game data and code (Godot 4.x + GDScript, JSON data). Use this skill whenever the user works on the FactoryDefence game — adding/editing items, recipes, facilities, modules, weapons, science packs; generating or syncing JSON data files from the GDD at reference/FactoryDefenceGame.md; writing GDScript data classes or loaders; assigning uint64 item IDs; or verifying recipe/ingredient consistency. Trigger on phrases like "add item", "new recipe", "facility data", "GDD에서 가져와", "아이템 추가", "레시피 추가", "JSON 데이터", "ID 할당", "데이터 동기화", or any work that touches the items/recipes/facilities defined in the GDD. Use this skill even when the user does not explicitly mention the GDD — if they are editing game data tables or factory/automation/recipe code in this project, this skill applies.
---

# Crafting Defence Game Data Skill

A coding-and-data skill for the Factory Defence game (Godot 4.x + GDScript). The Game Design Document at `reference/FactoryDefenceGame.md` is the **single source of truth** for every game item, recipe, and facility. JSON files in `res://data/` mirror it exactly; GDScript classes consume the JSON at runtime.

## Core principles

1. **GDD is source of truth.** When the JSON and the GDD disagree, the GDD wins — unless the user explicitly says the data is the new truth (in which case update the GDD too, and tell the user the GDD changed).
2. **JSON mirrors GDD exactly.** Same names, same numbers, same units. Do not invent fields, infer values, or "round" numbers from the GDD. If the GDD leaves a cell blank, ask the user — do not guess.
3. **Code references JSON, not magic numbers.** Recipe times, energy costs, pollution rates, speeds, and sizes must never be hardcoded in gameplay code — load them from the data layer.
4. **Every item has a uint64 ID.** Format: `[WORD][WORD][DWORD]`. See `references/id-encoding.md`. IDs are append-only — never reused.
5. **Files ≤ 500 lines.** Split by concern when approaching the limit.

## When this skill applies

- Adding a new item, recipe, or facility from the GDD into JSON
- Editing values (ingredients, time, energy, pollution, size, module slots)
- Generating or updating GDScript data classes (`scripts/data/`)
- Writing or modifying the JSON loader (`scripts/loaders/game_data_loader.gd`)
- Validating that ingredient references resolve and IDs are unique
- Assigning a new uint64 ID to a new item
- Writing gameplay code that needs accurate values (energy, pollution, speed) — the values come from data, not from re-deriving them

## Workflow: importing data from the GDD

1. **Locate it in the GDD.** Find the row(s) in `reference/FactoryDefenceGame.md`. If the row does not exist or has empty cells you need, **stop and ask the user** rather than inventing.
2. **Pick the right JSON file.** See `references/folder-structure.md`. Place by category (items / facilities / transport / storage / recipes).
3. **Assign an ID.** Follow `references/id-encoding.md`. Search every JSON data file for collisions before committing the ID.
4. **Match the schema.** See `references/data-schemas.md`. Required fields must be present; numeric units must match the GDD's units.
5. **Cross-check references.** Every ingredient and result name must resolve to an existing item. Every `produced_in` entry must resolve to an existing facility.
6. **Update GDScript only when shape changes.** Adding a new row of data does not require code changes. Adding a new field does — extend the matching class in `scripts/data/`.

## Workflow: writing gameplay code that uses data

1. **Load via the central loader** (`scripts/loaders/game_data_loader.gd`). Do not re-open JSON files in gameplay systems.
2. **Look up by item id (`int`), not by string.** Names are GDD/UX labels; IDs are runtime keys.
3. **Use the GDD's units.** Energy: kW for consumption, kJ for per-craft cost. Time: seconds. Pollution: per minute. Belt speed: items/second.
4. **Don't extrapolate.** If the GDD doesn't specify a value (e.g., "제작 장소" cell is blank), ask the user.

## Folder structure

Read `references/folder-structure.md` for the canonical `res://` layout. New data files go by category, not bundled.

## Data schemas

Read `references/data-schemas.md` for the required JSON shape per category — items, facilities, recipes, and shared sub-objects (ingredient, energy, results).

## ID encoding

Read `references/id-encoding.md` before assigning any new ID. The `[WORD][WORD][DWORD]` scheme encodes category hierarchically. A wrong ID is hard to fix later because gameplay code keys off it at runtime.

## GDScript patterns

Read `references/gdscript-patterns.md` for the loader pattern, data class shape, JSON parsing conventions, autoload setup, and the file-size split strategy.

## Project conventions (overrides of the raw GDD)

These rules win over GDD tables when they conflict. The GDD at `reference/FactoryDefenceGame.md` has been updated to reflect them.

- **Burner fuel = Coal (and SolidFuel) only.** Wood is **not** a fuel item, despite older drafts of the GDD listing it. Wood remains a craftable ingredient for WoodBox, UtilityPole, TreeSeed, Shotgun, CombatShotgun, etc. When adding code that checks fuel acceptance or generates UI for fuel slots, do not list Wood as an option.

## Validation checklist (before reporting a data change as done)

- [ ] All new items have unique uint64 IDs that match the category encoding scheme
- [ ] All ingredient and result references resolve to an existing item
- [ ] All `produced_in` references resolve to an existing facility
- [ ] Numeric fields use the GDD's units (kW vs kJ, /sec vs /min)
- [ ] No magic numbers in gameplay code — values come from JSON via the loader
- [ ] No source file exceeds 450 lines
- [ ] If the GDD itself was edited, the user is told explicitly (GDD edits are a big deal — they change the source of truth)
