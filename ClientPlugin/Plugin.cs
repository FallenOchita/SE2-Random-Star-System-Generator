using System;
using System.Reflection;
using HarmonyLib;
using Keen.VRage.Core.Plugins;
using Keen.VRage.Library.Diagnostics;
using RSSG.Core;

namespace RSSG.ClientPlugin;

public sealed class Plugin : IPlugin
{
    public const string Name = "RSSG";
    public static Plugin Instance { get; private set; }

    public Plugin()
    {
        Instance = this;
#if DEBUG
        Harmony.DEBUG = true;
#endif
        var harmony = new Harmony(Name);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        Log.Default.WriteLine($"[{Name}] Random Star System Generator loaded.");
        Log.Default.WriteLine($"[{Name}] Runtime generation preview is available from the plugin configuration action.");
    }

    // Pulsar invokes this method from the plugin's configuration button.
    // Until a verified SE2 world-generation hook is implemented, this is a safe preview only.
    public void OpenConfigDialog()
    {
        try
        {
            var settings = GenerationSettings.Default;
            var system = new StarSystemGenerator().Generate(settings);
            Log.Default.WriteLine($"[{Name}] Preview: {system.Name}");
            Log.Default.WriteLine($"[{Name}] Stars: {system.Stars.Count}; planets: {system.Planets.Count}; asteroid belts: {system.AsteroidBelts.Count}");
            foreach (var planet in system.Planets)
                Log.Default.WriteLine($"[{Name}] {planet.Name}: {planet.Class}, orbit {planet.OrbitalDistanceAu:F2} AU, moons {planet.Moons.Count}");
        }
        catch (Exception e)
        {
            Log.Default.WriteLine(LogSeverity.Error, $"[{Name}] Generation preview failed: {e}");
        }
    }
}
