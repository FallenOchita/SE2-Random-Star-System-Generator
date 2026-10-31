using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RSSG.Core;

/// <summary>
/// Versioned JSON envelope for storing generated system data alongside a world save.
/// This codec does not write to the game's save directory; the runtime adapter owns that step.
/// </summary>
public static class SystemSnapshotCodec
{
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string Serialize(GeneratedStarSystem system, string gameBuild)
    {
        ArgumentNullException.ThrowIfNull(system);
        if (string.IsNullOrWhiteSpace(gameBuild))
            throw new ArgumentException("A game build identifier is required.", nameof(gameBuild));

        ValidateSystem(system);
        var snapshot = new SystemSnapshot(CurrentSchemaVersion, gameBuild, system);
        return JsonSerializer.Serialize(snapshot, JsonOptions);
    }

    public static GeneratedStarSystem Deserialize(string json, string expectedGameBuild)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("Snapshot JSON cannot be empty.", nameof(json));
        if (string.IsNullOrWhiteSpace(expectedGameBuild))
            throw new ArgumentException("An expected game build identifier is required.", nameof(expectedGameBuild));

        var snapshot = JsonSerializer.Deserialize<SystemSnapshot>(json, JsonOptions)
            ?? throw new InvalidOperationException("Snapshot document was empty.");

        if (snapshot.SchemaVersion != CurrentSchemaVersion)
            throw new NotSupportedException($"Snapshot schema {snapshot.SchemaVersion} is not supported; expected {CurrentSchemaVersion}.");
        if (!string.Equals(snapshot.GameBuild, expectedGameBuild, StringComparison.Ordinal))
            throw new InvalidOperationException($"Snapshot targets SE2 {snapshot.GameBuild}, but the current build is {expectedGameBuild}.");
        if (snapshot.System is null)
            throw new InvalidOperationException("Snapshot does not contain a generated star system.");

        ValidateSystem(snapshot.System);
        return snapshot.System;
    }

    private static void ValidateSystem(GeneratedStarSystem system)
    {
        if (string.IsNullOrWhiteSpace(system.Name))
            throw new InvalidOperationException("Generated system name is missing.");
        if (system.Stars is null || system.Stars.Count is < 1 or > 2)
            throw new InvalidOperationException("Generated system must contain one or two stars.");
        if (system.Planets is null || system.Planets.Count is < 1 or > 32)
            throw new InvalidOperationException("Generated system must contain between one and 32 planets.");
        if (system.AsteroidBelts is null || system.AsteroidBelts.Count > 8)
            throw new InvalidOperationException("Generated system contains an invalid asteroid-belt count.");

        foreach (var planet in system.Planets)
        {
            if (planet is null || planet.Moons is null || planet.Moons.Count > 12)
                throw new InvalidOperationException("Generated system contains an invalid planet or moon collection.");
        }
    }

    private sealed record SystemSnapshot(int SchemaVersion, string GameBuild, GeneratedStarSystem System);
}
