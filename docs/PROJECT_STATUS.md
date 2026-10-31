# Project status and audit notes

**Last updated:** 2026-10-31  
**Default branch:** \`main\`  
**Validated feature branch:** \`feature/data-driven-workshop\`

## Repository baseline

- Godot 4.6 .NET project using C# and a .NET 8 desktop target.
- The default entry scene is \`Scenes/Workshop/workshop.tscn\`.
- The older \`player_mech.tscn\` preview scene remains available separately.
- Scripts are grouped into \`Combat\`, \`Core\`, \`Mech\`, and \`Mech/Workshop\`.
- A large existing sprite/audio collection is present under \`Sprites/\`; its provenance and redistribution permissions are not fully documented.

## Completed foundation work

- Registered \`MechStatManager\` as an autoload.
- Fixed the legacy preview scene's controller reference and assembler socket wiring.
- Separated build-budget cost from energy capacity in the existing mech stat layer.
- Improved component validation and managed socket replacement.
- Removed a hard-coded missing test resource dependency.
- Aligned xUnit with .NET 8 and added pure build-rule tests.
- Added repository instructions, architecture notes, and project status tracking.
- Added remote GitHub Actions validation for Godot 4.6 .NET and .NET 8.

## Implemented local workshop

- Added a default workshop scene with a wide three-panel Control-based interface and a narrow stacked/scrollable layout that switches below a 1024px viewport width.
- Added a project-authored starter catalogue with four slots: Frame, Mobility, Core, and Utility.
- Frame and Mobility are required; Core and Utility are optional.
- Added one item per slot, replacement behavior, optional-slot removal, and a reset-to-starter action.
- Added live derived statistics and validation for required slots, valid part data, weight, and build-cost limits.
- Added an original vector mech preview drawn with Godot canvas primitives; the workshop does not copy additional third-party artwork.
- Added a versioned JSON save payload and per-user \`user://mech_loadout.json\` persistence.
- Separated pure file persistence from the Godot path adapter so save/load behavior can be tested with temporary files.
- Added tests for catalogue integrity, stat aggregation, required-slot validation, JSON round-tripping, malformed/unsupported saves, file save/load, and safe handling of invalid replacement data.

All sample part names and balance values are project-authored prototype data. They are not represented as the original game's item database.

## Latest remote verification

The validated branch head \`fc351bf86d1547f9ec121614735bcf5873611782\` passed [GitHub Actions run 41227005160](https://github.com/Adeeb-muhammad/Super-Mech-/actions/runs/41227005160):

- Godot 4.6 .NET resource import: passed.
- C# build before and after import: passed, each with **0 warnings and 0 errors**.
- Automated tests: **24/24 passed**, with **0 warnings and 0 errors**.
- Default workshop scene headless launch: passed.

This verifies import, desktop compilation, the tested domain and file-persistence logic, and scene startup. It does not replace interactive visual QA or mobile aspect-ratio testing.

## Known limitations and next priorities

1. Complete interactive visual QA at common desktop and mobile aspect ratios; compact layout behavior now has pure breakpoint tests, but the actual rendered arrangement still needs visual inspection.
2. Add a multi-build save manager with build naming and duplicate/copy actions.
3. Add an inventory ownership model instead of treating every catalogue entry as immediately available.
4. Unify the new workshop loadout model with the older \`MechAssembler\` / \`MechPart\` preview path after compatibility tests.
5. Add the navigation shell and non-combat progression, campaign map, and reward screens.
6. Resolve third-party asset provenance or replace unclear assets with original/licensed alternatives.
7. Add automated UI interaction tests for filter selection, equip/replace/remove actions, save, load, and reset.

The repository is an improved, tested prototype, not a finished full game.


## Responsive workshop increment

- The workshop now re-parents the catalogue, preview, and details panels into a vertically stacked scroll container below the compact-layout breakpoint.
- The wide layout keeps the familiar three-panel arrangement; compact mode reduces margins, removes fixed side-panel widths, hides the offline badge on very narrow viewports, and gives the preview a lower minimum height.
- `WorkshopLayoutRules` contains the breakpoint and margin decisions and is tested independently. Actual visual testing at device aspect ratios remains outstanding.
