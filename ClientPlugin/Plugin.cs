using System;
using System.Reflection;
using HarmonyLib;
using Keen.VRage.Core.Plugins;
using Keen.VRage.Library.Diagnostics;
using RSSG.Core;\nusing System.IO;\nusing System.Text.Json;

namespace RSSG.ClientPlugin;

public sealed class Plugin : IPlugin
{
    public const string Name = "RSSG";
    public const string SupportedGameBuild = "4.0.0.3165";
    public static Plugin Instance { get; private set; }\n    public static GenerationSettings CurrentSettings { get; private set; } = GenerationSettings.Default;\n    private static readonly string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RSSG");\n    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "generation-settings.json");

    public Plugin()
    {
        Instance = this;
#if DEBUG
        Harmony.DEBUG = true;
#endif
        var harmony = new Harmony(Name);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        LoadSettings();\n        Log.Default.WriteLine($"[{Name}] Random Star System Generator loaded.");
        Log.Default.WriteLine($"[{Name}] Target SE2 build: {SupportedGameBuild}.");
        Log.Default.WriteLine($"[{Name}] Running read-only lifecycle and GUI API discovery.");
        Se2ApiDiscovery.DumpCandidates();
        Log.Default.WriteLine($"[{Name}] Runtime generation preview is available from the plugin configuration action.");
    }

    public void OpenConfigDialog()
    {
        try
        {
            var settings = GenerationSettings.Default;
            var system = new StarSystemGenerator().Generate(settings);
            Log.Default.WriteLine($"[{Name}] Preview: {system.Name}");\n            Log.Default.WriteLine($"[{Name}] GPS marker preference: {(CurrentSettings.ShowGpsMarkers ? "enabled" : "disabled")} (actual in-world GPS creation is not connected yet).");
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
