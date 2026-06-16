# Content ID Encoding

Every piece of game content (item, enemy, ability, recipe, etc.) is keyed by a **`ulong`** ID. The ID is **hierarchical** — its high bits encode the category, so the runtime can decode an ID back into a category without a separate lookup.

The convention comes from the project rule:

> 모든 item 및 자원은 unsigned int64 형태의 id key 를 갖는다. id key 는 [WORD][WORD][DWORD] 형태로 Category 정보를 포함하여 계층적으로 만들어 unique 하게 부여 되도록 한다.

## Layout

```
| HIGH WORD (16-bit) | MID WORD (16-bit) |     DWORD (32-bit)      |
|     Category       |  SubCategory/Tier |   Sequential index      |
```

Total: 64 bits, stored as C# `ulong`. In hex notation: `0xCCCC_SSSS_IIIIIIII`.

C# has a real `ulong` type, so the entire 64-bit unsigned range is usable. (Note: JSON numbers don't safely represent `ulong` above 2^53 — store IDs as **hex strings**, not numbers. See "JSON representation" below.)

## HIGH WORD — Category

Use **stable hex constants** defined in this file. Do **not** derive them from a C# enum's auto-numbering — if the enum is reordered later, IDs would silently shift.

This skill does not prescribe the category table. Each project defines its own list of categories here, with stable hex codes that never change once assigned. Example for a crafting/automation game:

| Category       | Code   |
| -------------- | ------ |
| Mineral        | 0x0001 |
| Natural        | 0x0002 |
| Fluid          | 0x0003 |
| Intermediate   | 0x0004 |
| Facility       | 0x0005 |
| Storage        | 0x0006 |
| Transport      | 0x0007 |
| Weapon         | 0x0008 |
| SciencePack    | 0x0009 |
| Module         | 0x000A |

For a different genre (e.g., an action RPG) the table might be `Item / Equipment / Consumable / Skill / Enemy / Quest / Dialogue / Loot`. Pick once, freeze, and commit.

Codes `0x0000` and `0xFFFF` are reserved (none/sentinel). Otherwise the full `0x0001..0xFFFE` range is available.

## MID WORD — SubCategory

Use `0x0000` if the category has no natural subdivision. Otherwise, define a per-category table in this file. Example for the `Facility` category above:

| SubCategory      | Code   |
| ---------------- | ------ |
| MiningDrill      | 0x0001 |
| Furnace          | 0x0002 |
| AssemblyMachine  | 0x0003 |
| Power            | 0x0004 |
| Pump             | 0x0005 |

If a new subcategory is needed, add it to this file **before** assigning IDs.

## DWORD — Sequential index

Starts at `0x00000001` within each `(Category, SubCategory)` pair. **Append-only.** Never reuse an index after deletion — leave gaps.

`0x00000000` is reserved for "none / unset". Do not assign it.

## JSON representation

`ulong` exceeds the safe-integer range of JSON parsers. Always write IDs as 16-character hex strings with the `0x` prefix:

```json
{ "id": "0x0004000000000005", "name": "IronPlate" }
```

The C# loader parses them via `ContentId.Parse` (see `csharp-patterns.md`).

## C# helpers

Provide a small static helper for bit-level work — no project should pass dictionaries pretending to be IDs.

```csharp
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
        return ulong.Parse(s, System.Globalization.NumberStyles.HexNumber);
    }

    public static string ToHex(ulong id) => $"0x{id:X16}";
}
```

## Examples

`IronPlate` (Intermediate, no subcategory, 5th item):
```
0x0004_0000_0000_0005   →   "0x0004000000000005"
```

`ElectricMiningDrill` (Facility / MiningDrill / 2nd):
```
0x0005_0001_0000_0002   →   "0x0005000100000002"
```

## Rules

1. **Append-only.** Once an ID is assigned and committed, never repurpose it — gameplay code, save files, and any deployed builds reference it.
2. **Validate uniqueness across all data files.** Before assigning a new ID, grep every data file for it.
3. **Decode helpers belong in code, not in data.** Don't store `category` redundantly in JSON when it's derivable from the ID — but a redundant `category` name field is fine as a human-readable sanity check.
4. **Mismatch = bug.** If `id`'s high word disagrees with the `category` field, the loader rejects it.
5. **All runtime keys are `ulong`, never strings.** Strings exist only at JSON boundaries and in display text.
