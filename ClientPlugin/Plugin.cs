using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using Keen.VRage.Core.Plugins;
using Keen.VRage.Library.Diagnostics;
using RSSG.Core;

namespace RSSG.ClientPlugin;

public sealed class Plugin : IPlugin
{
    public const string Name = "RSSG";
    public const string SupportedGameBuild = "4.0.0.3165";
    public static Plugin Instance { get; private set; }
    public static GenerationSettings CurrentSettings { get; private set; } = GenerationSettings.Default;

    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RSSG");
    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "generation-settings.json");

    public Plugin()
    {
        Instance = this;
#if DEBUG
        Harmony.DEBUG = true;
#endif
        var harmony = new Harmony(Name);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        LoadSettings();
        Log.Default.WriteLine($"[{Name}] Random Star System Generator loaded.");
        Log.Default.WriteLine($"[{Name}] Target SE2 build: {SupportedGameBuild}.");
        Log.Default.WriteLine($"[{Name}] Running read-only lifecycle and GUI API discovery.");
        Se2ApiDiscovery.DumpCandidates();
        Log.Default.WriteLine($"[{Name}] Current generation settings loaded. Open plugin Settings for configuration.");
    }

    // Pulsar invokes this from the plugin Settings action.
    // The actual native SE2 screen is added after the GUI type signatures are verified.
    public void OpenConfigDialog()
    {
        try
        {
            var system = new StarSystemGenerator().Generate(CurrentSettings);
            Log.Default.WriteLine($"[{Name}] Preview: {system.Name}");
            Log.Default.WriteLine($"[{Name}] Stars: {system.Stars.Count}; planets: {system.Planets.Count}; asteroid belts: {system.AsteroidBelts.Count}");
            Log.Default.WriteLine($"[{Name}] GPS marker preference: {(CurrentSettings.ShowGpsMarkers ? "enabled" : "disabled")}; in-world GPS creation is not connected yet.");
            foreach (var planet in system.Planets)
                Log.Default.WriteLine($"[{Name}] {planet.Name}: {planet.Class}, orbit {planet.OrbitalDistanceAu:F2} AU, moons {planet.Moons.Count}");
        }
        catch (Exception e)
        {
            Log.Default.WriteLine(LogSeverity.Error, $"[{Name}] Generation preview failed: {e}");
        }
    }

    private static void LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                CurrentSettings = GenerationSettings.Default;
                return;
            }

            var loaded = JsonSerializer.Deserialize<GenerationSettings>(File.ReadAllText(SettingsPath));
            if (loaded is null)
                throw new InvalidOperationException("Settings file did not contain a settings object.");
            loaded.Validate();
            CurrentSettings = loaded;
        }
        catch (Exception e)
        {
            CurrentSettings = GenerationSettings.Default;
            Log.Default.WriteLine($"[{Name}] Could not load settings; defaults restored: {e.Message}");
        }
    }

    internal static void SaveSettings(GenerationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();
        Directory.CreateDirectory(SettingsDirectory);
        string temporaryPath = SettingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporaryPath, SettingsPath, true);
        CurrentSettings = settings;
    }
}
