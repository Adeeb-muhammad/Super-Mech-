# Architecture overview

## Godot scene layer

\`player_mech.tscn\` is the current entry scene. It contains the visual mech root, socket nodes, static preview sprites, and a \`MechAssembler\` child.

\`PlayerMechController\` observes \`MechAssembler.MechChanged\` and refreshes the global stat aggregate.

## Mech data layer

- \`MechPart\` is a Godot Resource describing one component.
- \`MechAssembler\` attaches component sprites to configured sockets and maintains the managed equipped-part collection.
- \`MechValidator\` applies the Godot-facing validation and diagnostics.
- \`MechBuildRules\` contains pure C# budget rules that can be tested without starting the Godot runtime.
- \`MechStatManager\` aggregates component stats and is configured as an autoload.

Build-budget energy cost and in-game energy capacity are intentionally separate properties. Do not reuse one for the other.

## Combat layer

The repository contains combatant adapters, a turn coordinator, an AI coordinator, a UI listener, and a small constants class. These scripts are not currently wired into the configured main scene. Their integration should be done in a dedicated scene rather than adding unrelated gameplay nodes to the mech preview.

## Testing boundary

Pure calculations belong in regular C# helpers where practical. Godot scene behavior and resource loading require Godot-aware validation in addition to unit tests.
