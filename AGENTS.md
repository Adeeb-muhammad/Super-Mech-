# Repository development instructions

## Project baseline
- This is an unofficial fan project implemented with Godot 4.6 and C#.
- The game project targets .NET 8 (Android builds use the configured .NET 9 target).
- Open the project with the Godot 4.6 .NET editor. Do not open it with a non-.NET editor build.
- The main scene is currently a mech preview/assembly scene; do not describe the repository as a complete playable game.

## Before changing code
1. Read this file and \`docs/PROJECT_STATUS.md\`.
2. Inspect the scene, scripts, resources, and call sites affected by a proposed change.
3. Preserve stable resource UIDs and scene node paths unless a change intentionally updates every reference.
4. Keep game logic modular and avoid adding dependencies without a clear need.
5. Do not invent game balance values or claim exact parity with the original game without a reliable source.

## Godot and C# conventions
- Use Godot 4 APIs and C# scripts inheriting from the appropriate Godot base class.
- Godot-attached C# classes must be \`partial\`.
- Use typed exported properties for scene/resource references.
- Validate exported Node and Resource references before using them.
- Connect signals/events in \`_Ready\` and disconnect owned connections in \`_ExitTree\`.
- Keep pure rules/calculations independent of Godot where practical so they can be unit-tested.
- Keep test source under \`Tests/\`; the game project explicitly excludes that directory from compilation.
- Do not modify generated \`.godot/\`, \`bin/\`, or \`obj/\` content.

## Validation
- Run \`dotnet build "super mech.csproj"\`.
- Run \`dotnet test "Tests/Tests.csproj"\`.
- Open the project in Godot 4.6 .NET, allow imports to complete, and run the main scene.
- Inspect the Output and Debugger panels for parser, import, C#, and scene-reference errors.
- Report any checks that could not be run. Never state that an unrun test passed.

## Assets and licensing
- The provenance and redistribution rights of some legacy-looking sprite/audio files are not documented in this repository.
- Do not assume the root MIT license grants rights to third-party assets.
- Before adding or redistributing third-party content, record its source and license. Prefer original or clearly licensed replacements when rights are uncertain.
- Keep asset filenames and resource paths stable unless all references have been updated.

## Git workflow
- Never force-push or rewrite shared history.
- Prefer a focused branch for changes that have not been validated in the Godot editor.
- Review the diff and run the available checks before merging or pushing to the default branch.
- Do not commit credentials, local editor state, build output, or generated caches.
