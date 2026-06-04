# Recommended `res://` Layout

```
res://
├── data/                          # JSON data — mirrors the GDD
│   ├── items/
│   │   ├── minerals.json          # Coal, Stone, IronOre, ...
│   │   ├── naturals.json          # Wood, Fish, Jellynut, ...
│   │   ├── fluids.json            # Water, Steam, CrudeOil, ...
│   │   ├── intermediates.json     # IronPlate, CopperCable, ElectronicCircuit, ...
│   │   ├── modules.json           # SpeedModule, EfficiencyModule, ProductionModule
│   │   ├── weapons.json           # Grenade, Shotgun, FlamethrowerAmmo, ...
│   │   └── science_packs.json     # AutomationSciencePack, ..., UtilitySciencePack
│   ├── facilities/
│   │   ├── mining.json            # FireMiningDrill, ElectricMiningDrill, LargeMiningDrill
│   │   ├── furnaces.json          # StoneFurnace, IronFurnace, ElectricFurnace
│   │   ├── assembly.json          # AssemblyMachine1, 2, 3
│   │   ├── power.json             # Boiler, SteamEngine
│   │   └── pumps.json             # CoastalPump, DrillingRig, Feeder
│   ├── storage/
│   │   └── boxes.json             # WoodBox, IronBox, SteelBox, LogisticsBox
│   ├── transport/
│   │   ├── belts.json             # all Conveyor / Underground / Distributor tiers
│   │   ├── pipes.json             # Pipe, UndergroundPipe
│   │   └── poles.json             # UtilityPole
│   └── recipes/
│       ├── smelting.json          # furnace recipes
│       ├── crafting.json          # most assembly recipes
│       ├── chemistry.json         # oil / water / sulfur / acid
│       ├── extraction.json        # lava → molten, coal liquefaction
│       └── military.json          # weapons, ammo, science packs
├── scripts/
│   ├── data/                      # GDScript data classes (one type per file)
│   │   ├── item_id.gd
│   │   ├── item_category.gd
│   │   ├── energy_type.gd
│   │   ├── item_def.gd
│   │   ├── facility_def.gd
│   │   ├── transport_def.gd
│   │   ├── storage_def.gd
│   │   ├── recipe_def.gd
│   │   ├── ingredient_def.gd
│   │   └── energy_def.gd
│   ├── loaders/                   # JSON loaders + validators
│   │   ├── game_data_loader.gd    # autoloaded as `GameData`
│   │   ├── json_loader.gd
│   │   ├── item_loader.gd
│   │   ├── facility_loader.gd
│   │   └── recipe_loader.gd
│   ├── gameplay/                  # systems that consume data
│   └── ...
└── reference/
    └── FactoryDefenceGame.md      # GDD — source of truth, do not auto-edit
```

## Why split this way?

- **Small files.** Each JSON stays under ~300 lines, well below the 450-line cap.
- **Diffability.** Editing assembly machines doesn't churn the mining drill file.
- **Loader simplicity.** The loader iterates a fixed list of `(path, type)` pairs.
- **Matches GDD sections.** Folder boundaries follow the GDD (2.1 mining, 2.2 furnace, 2.5 assembly, 4 transport, 7 recipes…).

## When to add a new file vs. extend existing

Add a new file when:
- A new subcategory appears (e.g., a future "Reactor" facility family).
- An existing JSON approaches 300 lines.

Extend the existing file when:
- Adding the next tier in a family already in the file (e.g., a hypothetical TurboPipe goes into `transport/pipes.json`).
- Adding individual items to an existing category list.

## Naming

- Files: `snake_case.json` for data, `snake_case.gd` for code.
- GDScript `class_name`: `PascalCase` (one type per file; file name is the class name in snake_case).
- Item names inside JSON: match the GDD exactly (`IronPlate`, `ElectricMiningDrill`).
