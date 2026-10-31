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
    MaxAsteroidBelts = 1
};

var first = generator.Generate(settings);
var second = generator.Generate(settings);
Assert(first.Name == second.Name, "stable name");
Assert(first.Stars.SequenceEqual(second.Stars), "stable stars");
Assert(JsonSerializer.Serialize(first.Planets) == JsonSerializer.Serialize(second.Planets), "stable planets");
Assert(first.Planets.Count == 5, "planet count");
Assert(first.Planets.All(p => p.Moons.Count is >= 1 and <= 2), "moon range");
Assert(first.AsteroidBelts.Count == 1, "belt count");

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

Console.WriteLine("RSSG core smoke tests passed.");
