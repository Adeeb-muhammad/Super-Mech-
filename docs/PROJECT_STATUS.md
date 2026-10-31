# Project status and audit notes

**Audit date:** 2026-10-31  
**Default branch inspected:** `main`  
**Development branch:** `chore/foundation-audit-2026-10-31`

## Confirmed repository shape

- Godot 4.6 .NET project using C# and a .NET 8 desktop target.
- The configured main scene is `player_mech.tscn`.
- The main scene is a preassembled mech preview made from static `Sprite2D` nodes.
- Scripts are grouped into `Combat`, `Core`, and `Mech`.
- A large legacy-looking sprite/audio collection is present under `Sprites/`.
- A test project existed, but it targeted .NET 7, was not included in the solution, and had no project reference to the game project.
- No `.tres` component catalog or saved loadout data was present in the inspected repository tree.
- No `AGENTS.md` existed before this audit.

## Changes prepared on the audit branch

- Registered `MechStatManager` as an autoload so existing absolute lookups at `/root/MechStatManager` have a matching project singleton.
- Corrected the main scene's `PlayerMechController` path and UID to match the actual script file and its UID sidecar.
- Connected `MechAssembler` exported socket references in the main scene.
- Separated a component's build-budget `EnergyCost` from its in-game `EnergyCapacity`; the combat adapter now reads the capacity field for its maximum energy.
- Made component validation use overflow-safe arithmetic and reject negative component values.
- Made socket re-equipping replace a previously managed component instead of stacking managed sprites.
- Removed the hard-coded missing `res://Resources/TestTorso.tres` dependency from the optional test helper; it now accepts a resource through the Inspector.
- Aligned the test project with .NET 8, referenced the game project, added it to the solution, and added tests for the pure build-budget rules.
- Replaced stale setup instructions and documented repository/asset limitations.

## Current limitations

- The main scene remains a visual mech preview. It is not yet a complete game menu, workshop, campaign, arena, or playable battle scene.
- The preassembled preview sprites are not yet represented by `MechPart` resources, so the preview does not automatically produce a complete gameplay stat sheet.
- No component resource catalog, profile/save system, progression flow, reward loop, or end-to-end integration scene was found in the inspected tree.
- Combat scripts are separate from the current main scene. Their exported NodePaths and turn flow still need a dedicated integration scene and editor validation.
- The provenance and redistribution rights of files under `Sprites/` are not documented sufficiently to conclude that the repository's MIT license covers them.
- Godot editor launch, asset import, C# compilation, runtime scene checks, and `dotnet test` have not been run in this hosted editing session. Do not treat the new tests as passed until they are executed locally or in CI.

## Priority order

1. Run Godot 4.6 .NET import/build and fix any parser, UID, scene, or C# errors.
2. Add a small data-backed component catalog and make the mech preview consume it.
3. Add a dedicated integration scene for the existing combat/UI components and test turn transitions.
4. Add a versioned local save format for the mech layout and player settings.
5. Build a cohesive navigation shell and connect the workshop and gameplay flows.
6. Add automated scene smoke tests and CI validation.
7. Resolve third-party asset provenance or replace unclear assets with original/licensed alternatives.

## Remote validation

- Added a GitHub Actions workflow on the default branch that provisions Godot 4.6 .NET and .NET 8, imports the project, builds the solution, runs the test project, and launches the main scene headlessly. The first run against the default branch is in progress; the development branch is queued for the next push.
- Removed stale root-node scene property assignments that did not correspond to any exported properties on `PlayerMechController`.
