# Project status and audit notes

**Last updated:** 2026-10-31  
**Default branch:** \`main\`  
**Current development branch:** \`feature/data-driven-workshop\`

## Repository baseline

- Godot 4.6 .NET project using C# and a .NET 8 desktop target.
- The main entry scene is \`Scenes/Workshop/workshop.tscn\`.
- The original \`player_mech.tscn\` mech preview remains available as a separate scene.
- Scripts are grouped into \`Combat\`, \`Core\`, \`Mech\`, and \`Mech/Workshop\`.
- A large existing sprite/audio collection is present under \`Sprites/\`; its provenance and redistribution permissions are not fully documented.

## Implemented in the foundation pass

- Registered \`MechStatManager\` as an autoload.
- Fixed the player controller scene reference and exported assembler socket wiring in the legacy preview scene.
- Separated build-budget cost from in-game energy capacity in the existing mech stat layer.
- Improved component validation and managed socket replacement.
- Removed a hard-coded missing test resource dependency.
- Aligned the xUnit test project to .NET 8 and added build-rule tests.
- Added \`AGENTS.md\`, architecture notes, project status tracking, and corrected setup instructions.
- Added remote GitHub Actions validation for Godot 4.6 .NET and .NET 8.

## Workshop implementation in progress

- Added a new default workshop scene and a self-contained responsive Control-based interface.
- Added a small original prototype catalogue for Frame, Mobility, Core, and Utility components.
- Added derived stat calculation and validation for required slots, component-slot matching, weight, and build-cost limits.
- Added a vector mech preview using Godot drawing primitives rather than copying additional uncertain third-party art.
- Added versioned JSON loadout serialization and local \`user://mech_loadout.json\` persistence.
- Added unit tests for catalogue uniqueness, save/load round-trip, malformed or unsupported saves, aggregate stats, and required-slot validation.
- The catalogue values are intentionally labelled as project-authored prototype values, not the original game's item stats.

## Known limitations and next priorities

1. Run the first remote Godot import/build/test against \`feature/data-driven-workshop\`; resolve all compile and scene-load errors.
2. Add automated integration coverage for the workshop controller and actual file persistence.
3. Make the legacy \`MechAssembler\` consume the saved workshop loadout, or replace it with one unified assembly path after compatibility tests.
4. Add rename/load multiple builds and a simple inventory ownership model.
5. Add campaign/navigation shell and non-combat progression/reward interfaces.
6. Complete interactive visual QA for the workshop at desktop and mobile aspect ratios.
7. Resolve third-party asset provenance or replace unclear assets with original/licensed alternatives.

## Verified foundation baseline

The previously merged main branch passed Godot 4.6 .NET import, C# compilation (0 warnings / 0 errors), 8/8 xUnit tests, and headless main-scene startup in [run 41225392449](https://github.com/Adeeb-muhammad/Super-Mech-/actions/runs/41225392449).

The CI environment logged a non-fatal Android SDK editor-setting message during import. Android export has not been validated. A headless launch confirms startup and script initialization, not interactive visual quality.
