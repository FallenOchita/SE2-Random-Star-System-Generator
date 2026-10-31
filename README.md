# SE2 Random Star System Generator (RSSG)

A deterministic procedural star-system generator and Pulsar client-plugin project for Space Engineers 2.

> **Status: v0.2 development scaffold. Target game build: SE2 4.0.0.3165.** The .NET generation engine is separate from the game runtime. A Pulsar-compatible SE2 client-plugin entry point and PluginHub manifest draft are included. The plugin configuration action currently produces a generation preview in the game log. It does **not** yet create or replace a live world.

## Current features
- Repeatable seed-based generation using a project-owned SplitMix64 PRNG.
- Single-star and binary-star systems.
- Configurable planet, moon, and asteroid-belt counts.
- Star and planet classes, orbital distances, approximate radius, gravity, atmosphere, and temperature data.
- Generated system names and stable object IDs.
- Generation-setting validation.
- Versioned JSON snapshot codec for generated-system data, including an SE2 build compatibility check.
- Core smoke tests for deterministic generation and snapshot round-tripping.
- Console smoke tests and GitHub Actions build workflow.
- Pulsar client-plugin entry point targeting the SE2 Modern runtime.
- Startup log identifies the target SE2 build (4.0.0.3165).
- Plugin configuration action that logs a generated preview.
- Draft PluginHub-SE2 registration manifest.

## Repository layout
```
src/RSSG.Core/          Game-independent generation engine and data models
src/RSSG.Host/          Host-neutral integration contract (not a live game adapter)
ClientPlugin/           Pulsar client-plugin entry point
tests/RSSG.Core.Tests/   Dependency-free executable smoke tests
.github/workflows/      Core build workflow
RSSG.ClientPlugin.xml   PluginHub-SE2 manifest draft
docs/                   Assembly reconnaissance notes and integration plan
```

## Build the generator core
Requires the .NET 10 SDK.

```bash
dotnet build RSSG.sln --configuration Release
dotnet run --project tests/RSSG.Core.Tests/RSSG.Core.Tests.csproj --configuration Release
```

## Build the SE2 client plugin
Requirements:
- Space Engineers 2 installed, with the `Game2` folder available.
- .NET 10 SDK.
- Pulsar 2.2.0 or newer, using its SE2 `Modern` runtime.

Open `RSSG.ClientPlugin.sln` in Visual Studio or Rider. Set the local `Game2` and optional `Pulsar` paths in an untracked `Directory.Build.props.user` file (see the Pulsar client plugin template documentation). Build with `dotnet build RSSG.ClientPlugin.sln`.

The plugin can also be loaded from a Pulsar development folder. The manifest draft is not yet ready for PluginHub submission: update its `Commit` field to the reviewed release commit first.

## Current plugin behavior
When loaded, RSSG registers as a Pulsar client plugin. Its plugin configuration action generates a deterministic sample system and writes the star, planet, orbit, and moon summary to the game log. This is a development preview, not an in-game world-generation menu.

## Persistence groundwork
`SystemSnapshotCodec` serializes a generated system into a versioned JSON document with the target SE2 build. Deserialization checks the schema version, expected game build, and basic system structure. This is groundwork for save/load persistence only: it does not yet write into SE2's save format or create world entities. The future game adapter must decide where and when the snapshot is stored.

## Assembly reconnaissance for 4.0.0.3165
The user-supplied assembly set was inspected for managed metadata/string evidence. It exposes candidate lifecycle and procedural-generation types, including:
- `Keen.Game2.GameAppComponent` in `SpaceEngineers2.dll`, with symbols including `LoadGameAsync`, `TryGetWorldLoadArgsFromStartupArguments`, and `GetWorldLoadArgsForDefaultWorld`.
- `Keen.Game2.Game.SessionComponents.WorldSessionComponent` in `Game2.Game.dll`, including `LoadWorldSession`.
- `Keen.Game2.Simulation.GameSystems.ProceduralGeneration.GameProceduralGenerationConfiguration` and `WorldSeedProviderSessionComponent` in `Game2.Simulation.dll`.
- `Keen.VRage.Voxels.Components.VoxelPlanetsSessionComponent` in `Game2.Simulation.dll`.
- `IWorldSeedProvider` and `WorldLoadArgs` symbols in the VRage/game assemblies.

These are **candidate investigation points only**. A string/metadata scan does not establish method signatures, call order, accessibility, thread requirements, or whether a method is safe to patch. No Harmony patches have been added for these symbols. The next engineering step is full managed metadata/IL inspection and a local compile against the complete matching game installation before implementing any world mutation.

See [the assembly reconnaissance notes](docs/SE2-4.0.0.3165-assembly-notes.md).

## Important SE2 limitations
- The current Pulsar SE2 plugin model is client-side. Do not describe this as a dedicated-server plugin.
- The plugin does not yet hook new-world creation, alter world-generation settings, spawn planets/voxels, or synchronize a generated system across multiplayer clients.
- The host interface in `src/RSSG.Host` is an architectural contract only. It is not connected to SE2 runtime APIs.
- A real world-generation adapter requires exact method-signature and IL verification against the target game build. Do not guess Harmony patch targets or private method signatures.
- In-game scripting/Programmable Blocks are not part of this plugin.

## Roadmap
1. Complete managed metadata and IL inspection for the candidate new-world/session lifecycle.
2. Verify a stable extension point and determine whether generation configuration can be injected before procedural world setup.
3. Implement a safe preview/configuration screen in the game UI.
4. Hook generation into new-world creation only after the exact game method and data model are verified.
5. Determine how a player-hosted multiplayer world distributes world-generation results to joining clients.
6. Add actual planet/voxel creation, save/load persistence, and multiplayer regression tests.

## References
- [Official SE2 modding guides](https://2.spaceengineers2.com/modding-guides/)
- [Pulsar](https://github.com/SpaceGT/Pulsar)
- [SE2 client plugin template](https://github.com/CometWorks/client2-plugin-template)
- [SE2 plugin development guide](https://cometworks.github.io/plugin-dev/se2/)

## License
MIT.
