# Super Mech

An unofficial fan-made mech game prototype built with **Godot 4.6 .NET** and **C#**.

This repository is an evolving implementation, not a finished or verified 1-to-1 recreation. The current main scene is a mech preview/assembly scene. Several gameplay scripts exist, but a complete playable loop and progression flow are not yet connected.

## Requirements

- Godot **4.6 .NET** editor build
- .NET SDK **8.0** for desktop development
- Git

Use the .NET edition of Godot; the standard editor cannot build C# scripts.

## Clone and open

\`\`\`bash
git clone https://github.com/Adeeb-muhammad/Super-Mech-.git
cd Super-Mech-
\`\`\`

Open \`project.godot\` in Godot 4.6 .NET and allow the editor to import the project and build the C# assembly.

## Build and test

From the repository root:

\`\`\`bash
dotnet restore "super mech.csproj"
dotnet build "super mech.csproj"
dotnet test "Tests/Tests.csproj"
\`\`\`

The test project targets the same .NET 8 framework as the desktop game and references the game project. Test sources are excluded from the game assembly.

## Project structure

| Path | Responsibility |
| --- | --- |
| \`Scripts/Combat/\` | Turn flow, combatant adapters, combat UI and AI coordination |
| \`Scripts/Core/\` | Shared gameplay constants |
| \`Scripts/Mech/\` | Component resources, assembly, validation and stat aggregation |
| \`Tests/\` | Automated tests for pure C# rules |
| \`Sprites/\` | Existing visual and audio resources |
| \`docs/PROJECT_STATUS.md\` | Audited implementation status and outstanding work |
| \`AGENTS.md\` | Development and repository-maintenance instructions |

## Current architecture notes

- \`MechStatManager\` is registered as an autoload at \`/root/MechStatManager\`.
- \`MechPart.EnergyCost\` is the build-budget cost; \`EnergyCapacity\` contributes to the in-game energy reserve. They are separate concepts.
- A component can be equipped into a configured socket through \`MechAssembler.TryAttachPart\`. Re-equipping an already-managed socket replaces the prior managed component.
- The main scene's preassembled preview art is currently static. It is not yet a complete data-backed saved mech loadout.
- Combat scripts are present, but the main scene does not yet connect them into a complete playable combat screen.

## Asset and license notice

The repository contains a large collection of sprites and audio files whose provenance and redistribution permissions are not fully documented. The root MIT license should not be interpreted as granting rights to third-party content. Confirm the source and license of each third-party asset before redistribution; use original or clearly licensed replacements where permission is uncertain.

This is an unofficial fan project and is not affiliated with or endorsed by TactiSoft.
