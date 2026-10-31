using RSSG.Core;
using System.Text.Json;

static void Assert(bool x, string m)
{
    if (!x) throw new Exception("TEST FAILED: " + m);
}

var generator = new StarSystemGenerator();
var settings = GenerationSettings.Default with
{
    Seed = 24680,
    MinPlanets = 5,
    MaxPlanets = 5,
    MinMoonsPerPlanet = 1,
    MaxMoonsPerPlanet = 2,
    MinAsteroidBelts = 1,
    MaxAsteroidBelts = 1,
    FirstOrbitMinAu = 0.4,
    FirstOrbitMaxAu = 0.4,
    MoonOrbitMinKm = 50000,
    MoonOrbitMaxKm = 50000,
    AsteroidBeltWidthMinAu = 0.25,
    AsteroidBeltWidthMaxAu = 0.25,
    ShowGpsMarkers = false
};

var first = generator.Generate(settings);
var second = generator.Generate(settings);
Assert(first.Name == second.Name, "stable name");
Assert(first.Stars.SequenceEqual(second.Stars), "stable stars");
Assert(JsonSerializer.Serialize(first.Planets) == JsonSerializer.Serialize(second.Planets), "stable planets");
Assert(first.Planets.Count == 5, "planet count");
Assert(first.Planets.All(p => p.Moons.Count is >= 1 and <= 2), "moon range");
Assert(first.AsteroidBelts.Count == 1, "belt count");
Assert(Math.Abs(first.Planets[0].OrbitalDistanceAu - 0.4) < 0.000001, "configurable first orbit");
Assert(first.Planets.All(p => p.Moons.All(m => Math.Abs(m.OrbitalDistanceKm - 50000) < 0.000001)), "configurable moon orbit");
Assert(Math.Abs((first.AsteroidBelts[0].OuterRadiusAu - first.AsteroidBelts[0].InnerRadiusAu) - 0.25) < 0.000001, "configurable belt width");
Assert(!settings.ShowGpsMarkers, "GPS marker preference is configurable");

var snapshot = SystemSnapshotCodec.Serialize(first, "4.0.0.3165");
var restored = SystemSnapshotCodec.Deserialize(snapshot, "4.0.0.3165");
Assert(restored.Name == first.Name, "snapshot name round-trip");
Assert(restored.Stars.SequenceEqual(first.Stars), "snapshot stars round-trip");
Assert(JsonSerializer.Serialize(restored.Planets) == JsonSerializer.Serialize(first.Planets), "snapshot planets round-trip");

var wrongBuildRejected = false;
try
{
    SystemSnapshotCodec.Deserialize(snapshot, "4.0.0.3159");
}
catch (InvalidOperationException)
{
    wrongBuildRejected = true;
}
Assert(wrongBuildRejected, "snapshot rejects mismatched game build");

var invalidSettingsRejected = false;
try
{
    generator.Generate(settings with { MinPlanets = 9, MaxPlanets = 2 });
}
catch (ArgumentOutOfRangeException)
{
    invalidSettingsRejected = true;
}
Assert(invalidSettingsRejected, "settings validation");

var invalidOrbitRejected = false;
try
{
    generator.Generate(settings with { FirstOrbitMinAu = 3, FirstOrbitMaxAu = 2 });
}
catch (ArgumentOutOfRangeException)
{
    invalidOrbitRejected = true;
}
Assert(invalidOrbitRejected, "orbit range validation");

Console.WriteLine("RSSG core smoke tests passed.");
