# Architecture overview

## Entry scene

\`project.godot\` opens \`Scenes/Workshop/workshop.tscn\`. \`WorkshopScreen\` constructs the local workshop UI and coordinates selection, equipping, save/load, and derived-stat presentation.

## Workshop domain

- \`WorkshopPartDefinition\` describes one project-authored component.
- \`WorkshopCatalog\` contains the small starter catalogue. Its values are project-authored prototype balance values, not a transcription of the source game's item database.
- \`MechPartSlot\` defines Frame, Mobility, Core, and Utility.
- \`MechBuildRules\` contains pure stat aggregation and loadout validation.
- \`WorkshopLoadoutData\` is the versioned save payload.
- \`WorkshopLoadoutCodec\` validates and serializes the payload as JSON.
- \`WorkshopFileStore\` owns plain file-system save/load logic and can be unit-tested using temporary paths.
- \`WorkshopSaveStore\` adapts the file store to Godot's per-user \`user://\` path.
- \`MechWorkshopPreview\` draws an original vector mech preview using Godot canvas primitives.

## Existing mech assembly prototype

The older \`player_mech.tscn\` scene remains available separately. \`MechPart\`, \`MechAssembler\`, \`MechValidator\`, \`PlayerMechController\`, and the \`MechStatManager\` autoload are retained. That static preview is not yet connected to the new workshop save model.

Build-budget cost and in-game energy capacity are separate values and must not be conflated.

## Testing boundary

The xUnit project links pure workshop-domain, JSON codec, and file-store sources without loading Godot. The remote Godot CI workflow validates resource import, C# compilation, and headless startup of the configured workshop scene. Interactive UI behavior and visual layout still need hands-on QA and dedicated UI automation.


## Responsive layout

- The workshop uses a three-panel `HBoxContainer` at wider viewport widths.
- Below `WorkshopLayoutRules.CompactBreakpoint` (1024 pixels), its panels are re-parented into a vertically stacked `VBoxContainer` inside a `ScrollContainer`.
- Side-panel minimum widths are removed in compact mode, preview height is reduced, and horizontal margins shrink for very narrow windows.
- The pure breakpoint/margin policy is covered by xUnit. Rendered visual QA at actual device aspect ratios is still required.
