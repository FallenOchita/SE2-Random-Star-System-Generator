# SE2 Random Star System Generator (RSSG)

A deterministic, configurable star-system preview generator for Space Engineers 2, delivered as a Pulsar client plugin.

> **Status: v0.3.0 preview release. Target game build: SE2 4.0.0.3165.** RSSG generates deterministic star-system data and displays it in an in-game settings/preview screen. It does **not** currently create or replace live worlds, spawn planets or voxels, create in-world GPS markers, or synchronize generated systems in multiplayer.

## Current features

- Repeatable seed-based generation using a project-owned SplitMix64 PRNG.
- Single-star and binary-star systems.
- Configurable planet, moon, and asteroid-belt count ranges.
- Configurable first-planet orbit, planetary orbital spacing, moon orbital-distance range, and asteroid-belt width.
- Toggles for binary stars, moons, asteroid belts, and the GPS-marker preference.
- In-game Avalonia settings screen with Generate Preview and Save Settings actions.
- Settings persistence in Pulsar's per-plugin data directory, respecting the game's `-appData` location.
- Star and planet classes, orbital distances, approximate radius, gravity, atmosphere, and temperature data.
- Generated system names and stable object IDs.
- Generation-setting validation.
- Versioned JSON snapshot codec for generated-system data, including an SE2 build compatibility check.
- Core smoke tests and GitHub Actions build workflow.

## Repository layout

```
src/RSSG.Core/          Game-independent generation engine and data models
src/RSSG.Host/          Host-neutral integration contract (not a live game adapter)
ClientPlugin/           Pulsar client-plugin entry point and Avalonia preview screen
tests/RSSG.Core.Tests/  Dependency-free executable smoke tests
.github/workflows/      Core build workflow
RSSG.ClientPlugin.xml   PluginHub descriptor draft
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
- Pulsar 2.4.3 or newer, using its SE2 Modern runtime.

Open `RSSG.ClientPlugin.sln` in Visual Studio or Rider. Set the local `Game2` and optional `Pulsar` paths in an untracked `Directory.Build.props.user` file (see the Pulsar client-plugin template documentation). Build with `dotnet build RSSG.ClientPlugin.sln`.

The plugin can also be loaded from a Pulsar development folder. The official catalogue registration is maintained separately in `StarCpt/PluginHub-SE2`; the catalogue descriptor pins a specific source commit.

## In-game settings and preview

Use RSSG's Pulsar **Settings** action to open the in-game configuration screen. It exposes the seed, planet/moon/belt count ranges, orbital ranges, and generation toggles. Select **Generate Preview** to display the generated star system, or **Save Settings** to validate and persist the current options.

Settings are stored in the plugin-specific directory returned by Pulsar's injected `GetConfigPath("RSSG", null)` delegate. This keeps the file under Pulsar's data area and follows the game's configured `-appData` directory rather than writing to a fixed operating-system user-data folder.

The preview is data-only. The GPS option is a saved preference and does not currently create GPS markers in the game.

## SE2 integration limitations

- The current Pulsar SE2 plugin model is client-side; RSSG is not a dedicated-server plugin.
- RSSG does not yet hook new-world creation, alter live world-generation settings, spawn planets/voxels, write into the SE2 save format, or synchronize generated systems across multiplayer clients.
- The host interface in `src/RSSG.Host` is an architectural contract only; it is not connected to SE2 runtime APIs.
- A future world-generation adapter requires exact method-signature, IL, lifecycle, and authority verification against the target game build. No speculative Harmony world-generation patches are included.
- In-game scripting/Programmable Blocks are not part of this plugin.

## Roadmap

1. Complete managed metadata and IL inspection for the candidate new-world/session lifecycle.
2. Verify a stable extension point and determine whether generation configuration can be injected before procedural world setup.
3. Implement world-generation integration only after the exact game method and data model are verified.
4. Determine how a player-hosted multiplayer world distributes generation results to joining clients.
5. Add actual planet/voxel creation, save/load persistence, and multiplayer regression tests.

## References

- [Official SE2 modding guides](https://2.spaceengineers2.com/modding-guides/)
- [Pulsar](https://github.com/SpaceGT/Pulsar)
- [SE2 client plugin template](https://github.com/CometWorks/client2-plugin-template)
- [SE2 plugin development guide](https://cometworks.github.io/plugin-dev/se2/)

## License

GPL-3.0. See [LICENSE](LICENSE).
