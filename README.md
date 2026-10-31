# SE2 Random Star System Generator (RSSG)

A deterministic procedural star-system generation core for Space Engineers 2.

> **Status: v0.1 prototype.** The procedural generation engine and host integration contract are committed. This is not yet an installable SE2 plugin: a real Pulsar/VRAGE3 adapter, world-generation hook, menu UI, and live multiplayer replication still need to be implemented and tested against the current game SDK.

## Features in the prototype
- Repeatable seed-based generation using a project-owned SplitMix64 PRNG.
- Single-star and binary-star systems.
- Configurable planet, moon, and asteroid-belt counts.
- Star and planet classes, orbital distances, approximate radius, gravity, atmosphere, and temperature data.
- Generated system names and stable object IDs.
- Generation-setting validation.
- Console smoke tests and a GitHub Actions build workflow.
- Host-neutral integration contract for automatic new-world generation and an in-game menu action.
- Server-authority check and a client-publishing boundary in the host contract.

## Repository layout
```
src/RSSG.Core/       Game-independent generation engine and data models
src/RSSG.Host/       Host integration contract (no invented game API calls)
tests/RSSG.Core.Tests/  Dependency-free executable smoke tests
.github/workflows/   Build workflow
```

## Build
Requires the .NET 10 SDK.

```bash
dotnet build RSSG.sln --configuration Release
dotnet run --project tests/RSSG.Core.Tests/RSSG.Core.Tests.csproj --configuration Release
```

## Quick example
```csharp
using RSSG.Core;

var generator = new StarSystemGenerator();
var settings = GenerationSettings.Default with
{
    Seed = 481516,
    MinPlanets = 4,
    MaxPlanets = 8,
    AllowBinaryStars = true
};
GeneratedStarSystem system = generator.Generate(settings);
Console.WriteLine($"{system.Name}: {system.Planets.Count} planets");
```

The same seed and settings produce the same generated output for a given generator version. If a future algorithm change alters output, increment `CurrentGeneratorVersion`.

## Planned SE2 integration
1. Verify the current Pulsar plugin template, supported target framework, and runtime lifecycle.
2. Implement the automatic new-world generation hook using verified SE2 APIs.
3. Add an in-game configuration/generation menu using supported UI hooks.
4. Make the server authoritative and replicate the generated system data to clients.
5. Test planet/voxel/entity creation in a real SE2 test world and package the compatible plugin.

## Important limitations
- This prototype generates star-system data; it does not spawn VRAGE3 planets, terrain, voxel asteroids, skyboxes, or physics entities.
- Multiplayer compatibility is not yet implemented or tested. The host contract only establishes the intended server-authoritative boundary.
- No SE1 APIs are targeted.
- Do not install this repository as a game plugin yet.

## License
This repository currently has the GNU GPL-3.0 license selected on GitHub. See [LICENSE](LICENSE).
