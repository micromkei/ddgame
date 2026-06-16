---
name: unity-game-data
description: Manage data-driven game content in Unity (C# + JSON / ScriptableObject). Use this skill whenever the user works on tabular game data — items, recipes, abilities, enemies, weapons, levels, drops, loot tables, stats, etc. — in a Unity/C# project. It applies to adding or editing data rows, generating or syncing JSON files from a design document, writing C# data classes and loaders, assigning hierarchical uint64 IDs, or validating cross-references between data files. Trigger on phrases like "add item", "new recipe", "데이터 추가", "JSON 데이터", "ID 할당", "데이터 동기화", "ScriptableObject", "data table", or when editing files under `Assets/Data/`, `Assets/StreamingAssets/Data/`, or `Assets/Scripts/Data/`. Also use when writing gameplay code that should read tuning values from data rather than hardcoding magic numbers.
---

# Unity Game Data Skill

A generic data-and-code skill for Unity (C# + JSON, optionally ScriptableObjects). The Game Design Document (GDD) is the **single source of truth** for every piece of game content. JSON/SO assets mirror it exactly; C# classes consume them at runtime.

This skill is language- and content-agnostic. The examples below use items/recipes for concreteness, but the same patterns work for enemies, abilities, levels, dialogue, loot tables, or any tabular game data.

## Core principles

1. **GDD is source of truth.** When data and GDD disagree, the GDD wins — unless the user explicitly says the data is the new truth (in which case update the GDD too, and tell the user the GDD changed).
2. **Data mirrors GDD exactly.** Same names, same numbers, same units. Do not invent fields, infer values, or "round" numbers. If the GDD leaves a cell blank, ask the user — do not guess.
3. **Code references data, not magic numbers.** Costs, times, speeds, sizes, damage values must never be hardcoded in gameplay code — load them from the data layer.
4. **Every content entry has a `ulong` ID.** Format: `[WORD][WORD][DWORD]`. See `references/id-encoding.md`. IDs are append-only — never reused.
5. **Identifiers, not strings, at runtime.** String compare is allowed only at (1) display-text and (2) data-boundary (JSON load/save). Everywhere else compare by `ulong` id or `enum`. Convert strings to ids/enums once at load time and cache.
6. **Files ≤ 500 lines.** Split by concern when approaching the limit.

## When this skill applies

- Adding a new row to a data table from the GDD into JSON (or a ScriptableObject asset)
- Editing values (costs, time, stats, sizes, slots)
- Generating or updating C# data classes (`Assets/Scripts/Data/`)
- Writing or modifying the JSON/SO loader (`Assets/Scripts/Data/Loaders/`)
- Validating that cross-references resolve and IDs are unique
- Assigning a new `ulong` ID to a new entry
- Writing gameplay code that needs tuning values — pull them from the data layer

## Workflow: importing data from the GDD

1. **Locate it in the GDD.** Find the row(s). If the row does not exist or has empty cells you need, **stop and ask the user** rather than inventing.
2. **Pick the right data file.** See `references/folder-structure.md`. Place by category, not bundled.
3. **Assign an ID.** Follow `references/id-encoding.md`. Search every data file for collisions before committing.
4. **Match the schema.** See `references/data-schemas.md`. Required fields must be present; numeric units must match the GDD's units.
5. **Cross-check references.** Every referenced name (ingredient, drop, prerequisite, etc.) must resolve to an existing entry.
6. **Update C# only when shape changes.** Adding a new row does not require code changes. Adding a new field does — extend the matching class in `Assets/Scripts/Data/`.

## Workflow: writing gameplay code that uses data

1. **Load via the central registry** (e.g. `DataRegistry`/`GameData`). Do not re-open JSON files in gameplay systems.
2. **Look up by id (`ulong`), not by name string.** Names are GDD/UX labels; IDs are runtime keys.
3. **Use the GDD's units.** Match them in the field names (`damagePerSec`, `costGold`, `cooldownSec`, …) so the unit is impossible to misread.
4. **Don't extrapolate.** If the GDD doesn't specify a value, ask the user.

## Folder structure

Read `references/folder-structure.md` for the canonical Unity layout (`Assets/StreamingAssets/Data/` for JSON, `Assets/Data/` for ScriptableObject assets, `Assets/Scripts/Data/` for C#).

## Data schemas

Read `references/data-schemas.md` for the JSON shape conventions — required vs optional fields, units, naming, the `{ "entries": [ ... ] }` envelope, and how to extend a base schema for specialized content.

## ID encoding

Read `references/id-encoding.md` before assigning any new ID. The `[WORD][WORD][DWORD]` scheme encodes category hierarchically; a wrong ID is hard to fix later because gameplay code, save files, and analytics key off it at runtime. Category/subcategory hex constants are **per-project** — defined once in `id-encoding.md` of the actual project, not invented ad-hoc.

## C# patterns

Read `references/csharp-patterns.md` for the data class shape, JSON parsing conventions (JsonUtility / Newtonsoft / System.Text.Json — pick one per project), the JSON-vs-ScriptableObject decision, the `DataRegistry` singleton pattern, and how to split the loader as it grows.

## Validation checklist (before reporting a data change as done)

- [ ] All new entries have unique `ulong` IDs that match the category encoding scheme
- [ ] All cross-references resolve to an existing entry
- [ ] Numeric fields use the GDD's units (the unit is in the field name)
- [ ] No magic numbers in gameplay code — values come from the data registry
- [ ] No source file exceeds 450 lines
- [ ] If the GDD itself was edited, the user is told explicitly (GDD edits are a big deal — they change the source of truth)
