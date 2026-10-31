using System;
using System.Collections.Generic;

namespace RSSG.Core;

public sealed class StarSystemGenerator
{
    public const int CurrentGeneratorVersion = 2;
    private static readonly string[] A = ["Aster", "Cinder", "Vesper", "Orion", "Nexa", "Helio", "Kestrel", "Solara", "Draco", "Aquila"];
    private static readonly string[] B = ["Reach", "Haven", "Crown", "Drift", "Point", "Vale", "Crossing", "Sanctum", "Horizon", "Gate"];

    public GeneratedStarSystem Generate(GenerationSettings s)
    {
        ArgumentNullException.ThrowIfNull(s);
        s.Validate();
        var r = new SeededRandom(s.Seed);
        string name = $"{Pick(r, A)} {Pick(r, B)}";
        var stars = new List<Star>();
        var starClass = (StarClass)r.NextInt(0, 5);
        stars.Add(MakeStar(r, "STAR-0", "Primary", starClass));
        if (s.AllowBinaryStars && r.NextBool(.22))
            stars.Add(MakeStar(r, "STAR-1", "Companion", (StarClass)r.NextInt(0, 3)));

        int count = r.NextInt(s.MinPlanets, s.MaxPlanets + 1);
        var planets = new List<Planet>();
        double orbit = r.NextDouble(s.FirstOrbitMinAu, s.FirstOrbitMaxAu);
        for (int i = 0; i < count; i++)
        {
            if (i > 0) orbit *= r.NextDouble(s.OrbitalSpacingMinMultiplier, s.OrbitalSpacingMaxMultiplier);
            var type = (PlanetClass)r.NextInt(0, 7);
            double radius = type == PlanetClass.GasGiant ? r.NextDouble(25000, 80000) :
                type == PlanetClass.IceGiant ? r.NextDouble(12000, 30000) : r.NextDouble(1800, 9000);
            double gravity = type == PlanetClass.GasGiant ? r.NextDouble(.8, 2.5) :
                type == PlanetClass.IceGiant ? r.NextDouble(.6, 1.6) : r.NextDouble(.15, 1.8);
            bool atmosphere = type is PlanetClass.GasGiant or PlanetClass.IceGiant || r.NextBool(.68);
            double temperature = 35 - Math.Log10(Math.Max(orbit, .1)) * 95 + r.NextDouble(-35, 35);
            int moonCount = s.GenerateMoons ? r.NextInt(s.MinMoonsPerPlanet, s.MaxMoonsPerPlanet + 1) : 0;
            var moons = new List<Moon>();
            for (int moon = 0; moon < moonCount; moon++)
                moons.Add(new Moon($"P{i + 1}-M{moon + 1}", $"Moon {i + 1}-{moon + 1}",
                    r.NextDouble(250, 4200), r.NextDouble(s.MoonOrbitMinKm, s.MoonOrbitMaxKm)));
            planets.Add(new Planet($"PLANET-{i + 1}", $"World {i + 1}", type, radius, gravity,
                atmosphere, temperature, orbit, moons));
        }

        var belts = new List<AsteroidBelt>();
        if (s.GenerateAsteroidBelts)
        {
            int beltCount = r.NextInt(s.MinAsteroidBelts, s.MaxAsteroidBelts + 1);
            for (int i = 0; i < beltCount; i++)
            {
                double inner = r.NextDouble(.8, Math.Max(1, planets[^1].OrbitalDistanceAu + 1));
                double width = r.NextDouble(s.AsteroidBeltWidthMinAu, s.AsteroidBeltWidthMaxAu);
                belts.Add(new AsteroidBelt($"BELT-{i + 1}", inner, inner + width, r.NextInt(100, 5001)));
            }
        }

        return new GeneratedStarSystem(s.Seed, CurrentGeneratorVersion, name, stars, planets, belts);
    }

    private static Star MakeStar(SeededRandom r, string id, string label, StarClass type)
    {
        (double minMass, double maxMass, double minRadius, double maxRadius, double minTemp, double maxTemp) = type switch
        {
            StarClass.RedDwarf => (.08, .6, .1, .6, 2400, 3900),
            StarClass.OrangeDwarf => (.6, .9, .7, .9, 3900, 5200),
            StarClass.YellowDwarf => (.9, 1.1, .9, 1.2, 5200, 6000),
            StarClass.WhiteStar => (1.1, 2, 1.2, 2, 7500, 12000),
            _ => (8, 18, 3, 8, 12000, 30000)
        };
        return new Star(id, $"{label} {r.NextInt(100, 999)}", type,
            r.NextDouble(minMass, maxMass), r.NextDouble(minRadius, maxRadius),
            r.NextDouble(minTemp, maxTemp));
    }

    private static string Pick(SeededRandom r, string[] values) => values[r.NextInt(0, values.Length)];
}
