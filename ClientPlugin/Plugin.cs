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

#pragma warning disable CS0649 // Pulsar injects this delegate before constructing the plugin.
    private static Func<string, string, string> GetConfigPath;
#pragma warning restore CS0649

    // Pulsar's per-plugin data directory respects the game's -appData location.
    public string DataDir { get; private set; } = GetConfigPath(Name, null);
    private static string SettingsPath { get; set; }

    public Plugin()
    {
        Instance = this;
        SettingsPath = Path.Combine(DataDir, "generation-settings.json");

#if DEBUG
        Harmony.DEBUG = true;
#endif
        var harmony = new Harmony(Name);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        LoadSettings();
        Log.Default.WriteLine($"[{Name}] Random Star System Generator loaded.");
        Log.Default.WriteLine($"[{Name}] Target SE2 build: {SupportedGameBuild}.");
        Log.Default.WriteLine($"[{Name}] Settings directory: {DataDir}");
        Log.Default.WriteLine($"[{Name}] Open plugin Settings to configure options and generate a preview.");
    }

    // Invoked by Pulsar via reflection when the player clicks the plugin Settings action.
    public void OpenConfigDialog()
    {
        try
        {
            var sharedUi = GameAccess.GetSharedUI();
            if (sharedUi == null)
            {
                Log.Default.WriteLine(LogSeverity.Warning, $"[{Name}] SharedUIComponent is not available yet.");
                return;
            }

            var viewModel = new PreviewSettingsScreenViewModel(
                "Random Star System Generator",
                panel => PreviewSettingsScreenViewModel.Populate(panel));

            sharedUi.CreateScreen<PreviewSettingsScreen>(viewModel, showCursor: true);
        }
        catch (Exception e)
        {
            Log.Default.WriteLine(LogSeverity.Error, $"[{Name}] Could not open settings screen: {e}");
        }
    }

    private static void LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                SaveSettings(GenerationSettings.Default);
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
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
        string temporaryPath = SettingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporaryPath, SettingsPath, true);
        CurrentSettings = settings;
    }

    internal static GeneratedStarSystem GeneratePreview(GenerationSettings settings) =>
        new StarSystemGenerator().Generate(settings);
}
