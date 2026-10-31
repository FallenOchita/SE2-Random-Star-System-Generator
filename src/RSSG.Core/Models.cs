using System;
using System.Collections.Generic;

namespace RSSG.Core;

public enum StarClass { RedDwarf, OrangeDwarf, YellowDwarf, WhiteStar, BlueGiant }
public enum PlanetClass { Terrestrial, Desert, Ocean, Ice, Volcanic, GasGiant, IceGiant }
public enum GenerationMode { AutomaticNewWorld, InGameMenu }

/// <summary>
/// Player-adjustable generation options. The numeric ranges are intended to map to
/// discrete in-game sliders; all distances are in AU or km as named.
/// </summary>
public sealed record GenerationSettings
{
    public int Seed { get; init; } = 20261030;
    public int MinPlanets { get; init; } = 3;
    public int MaxPlanets { get; init; } = 8;
    public int MinMoonsPerPlanet { get; init; } = 0;
    public int MaxMoonsPerPlanet { get; init; } = 3;
    public int MinAsteroidBelts { get; init; } = 0;
    public int MaxAsteroidBelts { get; init; } = 2;
    public bool AllowBinaryStars { get; init; } = true;
    public bool GenerateMoons { get; init; } = true;
    public bool GenerateAsteroidBelts { get; init; } = true;
    public bool ShowGpsMarkers { get; init; } = true;

    public double FirstOrbitMinAu { get; init; } = 0.25;
    public double FirstOrbitMaxAu { get; init; } = 0.65;
    public double OrbitalSpacingMinMultiplier { get; init; } = 1.35;
    public double OrbitalSpacingMaxMultiplier { get; init; } = 2.15;
    public double MoonOrbitMinKm { get; init; } = 30000;
    public double MoonOrbitMaxKm { get; init; } = 900000;
    public double AsteroidBeltWidthMinAu { get; init; } = 0.15;
    public double AsteroidBeltWidthMaxAu { get; init; } = 0.8;

    public static GenerationSettings Default => new();

    public void Validate()
    {
        if (Seed < 0) throw new ArgumentOutOfRangeException(nameof(Seed), "Seed must be zero or greater.");
        if (MinPlanets < 1 || MaxPlanets < MinPlanets || MaxPlanets > 32)
            throw new ArgumentOutOfRangeException(nameof(MaxPlanets), "Planet count must be between 1 and 32.");
        if (MinMoonsPerPlanet < 0 || MaxMoonsPerPlanet < MinMoonsPerPlanet || MaxMoonsPerPlanet > 12)
            throw new ArgumentOutOfRangeException(nameof(MaxMoonsPerPlanet), "Moon count must be between 0 and 12.");
        if (MinAsteroidBelts < 0 || MaxAsteroidBelts < MinAsteroidBelts || MaxAsteroidBelts > 8)
            throw new ArgumentOutOfRangeException(nameof(MaxAsteroidBelts), "Asteroid belt count must be between 0 and 8.");
        ValidateRange(FirstOrbitMinAu, FirstOrbitMaxAu, 0.05, 20, nameof(FirstOrbitMinAu));
        ValidateRange(OrbitalSpacingMinMultiplier, OrbitalSpacingMaxMultiplier, 1.05, 4, nameof(OrbitalSpacingMinMultiplier));
        ValidateRange(MoonOrbitMinKm, MoonOrbitMaxKm, 1000, 10000000, nameof(MoonOrbitMinKm));
        ValidateRange(AsteroidBeltWidthMinAu, AsteroidBeltWidthMaxAu, 0.01, 5, nameof(AsteroidBeltWidthMinAu));
    }

    private static void ValidateRange(double min, double max, double allowedMin, double allowedMax, string name)
    {
        if (double.IsNaN(min) || double.IsInfinity(min) || double.IsNaN(max) || double.IsInfinity(max) ||
            min < allowedMin || max > allowedMax || max < min)
            throw new ArgumentOutOfRangeException(name, $"Range must be ordered and within {allowedMin}..{allowedMax}.");
    }
}

public sealed record Star(string Id, string Name, StarClass Class, double SolarMasses, double RadiusSolar, double TemperatureKelvin);
public sealed record Moon(string Id, string Name, double RadiusKm, double OrbitalDistanceKm);
public sealed record Planet(string Id, string Name, PlanetClass Class, double RadiusKm, double GravityG, bool HasAtmosphere, double MeanTemperatureC, double OrbitalDistanceAu, IReadOnlyList<Moon> Moons);
public sealed record AsteroidBelt(string Id, double InnerRadiusAu, double OuterRadiusAu, int EstimatedMajorAsteroids);
public sealed record GeneratedStarSystem(int Seed, int GeneratorVersion, string Name, IReadOnlyList<Star> Stars, IReadOnlyList<Planet> Planets, IReadOnlyList<AsteroidBelt> AsteroidBelts);
