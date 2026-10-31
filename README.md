# SE2 Random Star System Generator (RSSG)

A deterministic procedural star-system generator and Pulsar client-plugin project for Space Engineers 2.

> **Status: v0.2 development scaffold.** The .NET generation engine is separate from the game runtime. A Pulsar-compatible SE2 client-plugin entry point and PluginHub manifest draft are now included. The plugin configuration action currently produces a generation preview in the game log. It does **not** yet create or replace a live world.

## Current features
- Repeatable seed-based generation using a project-owned SplitMix64 PRNG.
- Single-star and binary-star systems.
- Configurable planet, moon, and asteroid-belt counts.
- Star and planet classes, orbital distances, approximate radius, gravity, atmosphere, and temperature data.
- Generated system names and stable object IDs.
- Generation-setting validation.
- Console smoke tests and GitHub Actions build workflow.
- Pulsar client-plugin entry point targeting the SE2 Modern runtime.
- Plugin configuration action that logs a generated preview.
- Draft PluginHub-SE2 registration manifest.

## Repository layout
```
src/RSSG.Core/          Game-independent generation engine and data models
src/RSSG.Host/          Host-neutral integration contract (not a live game adapter)
ClientPlugin/           Pulsar client-plugin entry point
tests/RSSG.Core.Tests/   Dependency-free executable smoke tests
.github/workflows/       Core build workflow
RSSG.ClientPlugin.xml    PluginHub-SE2 manifest draft
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

## Important SE2 limitations
- SE2 currently has no dedicated server; the available Pulsar SE2 plugin model is client-side. Do not describe this as a dedicated-server plugin.
- The plugin does not yet hook new-world creation, alter world-generation settings, spawn planets/voxels, or synchronize a generated system across multiplayer clients.
- The host interface in `src/RSSG.Host` is an architectural contract only. It is not connected to SE2 runtime APIs.
- A real world-generation adapter requires inspection and testing against the exact current SE2 game assemblies. Do not guess Harmony patch targets or private method signatures.
- In-game scripting/Programmable Blocks are not part of this plugin.

## Roadmap
1. Verify the SE2 world creation flow and find a stable, supported runtime extension point.
2. Implement a safe preview/configuration screen in the game UI.
3. Hook generation into new-world creation only after the exact game method and data model are verified.
4. Determine how a player-hosted multiplayer world distributes world-generation results to joining clients.
5. Add actual planet/voxel creation, save/load persistence, and multiplayer regression tests.

## References
- [Official SE2 modding guides](https://2.spaceengineersgame.com/modding-guides/)
- [Pulsar](https://github.com/SpaceGT/Pulsar)
- [SE2 client plugin template](https://github.com/CometWorks/client2-plugin-template)
- [SE2 plugin development guide](https://cometworks.github.io/plugin-dev/se2/)

## License
MIT.
