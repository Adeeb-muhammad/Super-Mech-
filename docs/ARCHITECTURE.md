# Architecture overview

## Entry scene

\`project.godot\` opens \`Scenes/Workshop/workshop.tscn\`. \`WorkshopScreen\` builds the local workshop UI in code and owns the current interaction state.

## Workshop domain

- \`WorkshopPartDefinition\` describes a project-authored workshop component.
- \`WorkshopCatalog\` contains the small starter catalogue. Its values are prototype balance values, not a transcription of the source game's database.
- \`MechPartSlot\` defines the non-combat equipment slots: Frame, Mobility, Core, and Utility.
- \`WorkshopLoadoutData\` is the versioned save payload.
- \`WorkshopLoadoutCodec\` validates and serializes the payload as JSON.
- \`WorkshopSaveStore\` reads/writes the payload under Godot's per-user \`user://\` location.
- \`MechBuildRules\` calculates derived stats, checks required slots, and enforces the prototype build limits.
- \`MechWorkshopPreview\` draws an original vector mech preview using Godot canvas primitives.

## Existing mech assembly prototype

The older \`player_mech.tscn\` scene remains in the repository. \`MechPart\`, \`MechAssembler\`, \`MechValidator\`, \`PlayerMechController\`, and the \`MechStatManager\` autoload are retained. That scene's static preview is not yet connected to the new workshop save model.

Build-budget cost and in-game energy capacity are separate values and must not be conflated.

## Testing boundary

The test project links pure workshop-domain sources and uses xUnit without the Godot SDK. Godot-specific drawing, controls, and user:// file operations are checked by the remote Godot CI import/build/headless startup workflow; interactive layout and save/load testing still require further smoke-test coverage.
