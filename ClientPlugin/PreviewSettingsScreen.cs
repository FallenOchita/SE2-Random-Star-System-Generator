using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Keen.VRage.UI.Screens;
using RSSG.Core;

namespace RSSG.ClientPlugin;

internal sealed class PreviewSettingsScreenViewModel : ScreenViewModel
{
    public string Title { get; }
    public Action<StackPanel> BuildContent { get; }

    public PreviewSettingsScreenViewModel(string title, Action<StackPanel> buildContent)
    {
        KeepsOtherScreensVisible = false;
        AllowsInputBelowUI = false;
        AllowsInputFromLowerScreens = false;
        Title = title;
        BuildContent = buildContent;
        InitializeInputContext();
    }

    public static void Populate(StackPanel host)
    {
        host.Children.Clear();
        var settings = Plugin.CurrentSettings;

        host.Children.Add(new TextBlock
        {
            Text = "PREVIEW ONLY — this plugin does not create or modify live game worlds yet.",
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12)
        });

        var fields = new SettingsFields();
        AddField(host, "Seed", settings.Seed.ToString(), value => fields.Seed = value);
        AddField(host, "Minimum planets", settings.MinPlanets.ToString(), value => fields.MinPlanets = value);
        AddField(host, "Maximum planets", settings.MaxPlanets.ToString(), value => fields.MaxPlanets = value);
        AddField(host, "Minimum moons per planet", settings.MinMoonsPerPlanet.ToString(), value => fields.MinMoons = value);
        AddField(host, "Maximum moons per planet", settings.MaxMoonsPerPlanet.ToString(), value => fields.MaxMoons = value);
        AddField(host, "Minimum asteroid belts", settings.MinAsteroidBelts.ToString(), value => fields.MinBelts = value);
        AddField(host, "Maximum asteroid belts", settings.MaxAsteroidBelts.ToString(), value => fields.MaxBelts = value);
        AddField(host, "First orbit minimum (AU)", settings.FirstOrbitMinAu.ToString("0.##"), value => fields.FirstOrbitMin = value);
        AddField(host, "First orbit maximum (AU)", settings.FirstOrbitMaxAu.ToString("0.##"), value => fields.FirstOrbitMax = value);
        AddField(host, "Orbit spacing minimum", settings.OrbitalSpacingMinMultiplier.ToString("0.##"), value => fields.SpacingMin = value);
        AddField(host, "Orbit spacing maximum", settings.OrbitalSpacingMaxMultiplier.ToString("0.##"), value => fields.SpacingMax = value);
        AddField(host, "Moon orbit minimum (km)", settings.MoonOrbitMinKm.ToString("0"), value => fields.MoonOrbitMin = value);
        AddField(host, "Moon orbit maximum (km)", settings.MoonOrbitMaxKm.ToString("0"), value => fields.MoonOrbitMax = value);
        AddField(host, "Belt width minimum (AU)", settings.AsteroidBeltWidthMinAu.ToString("0.##"), value => fields.BeltWidthMin = value);
        AddField(host, "Belt width maximum (AU)", settings.AsteroidBeltWidthMaxAu.ToString("0.##"), value => fields.BeltWidthMax = value);

        var binaryStars = AddCheck(host, "Allow binary stars", settings.AllowBinaryStars);
        var generateMoons = AddCheck(host, "Generate moons", settings.GenerateMoons);
        var generateBelts = AddCheck(host, "Generate asteroid belts", settings.GenerateAsteroidBelts);
        var gpsMarkers = AddCheck(host, "Show GPS markers (preference only)", settings.ShowGpsMarkers);

        var result = new TextBlock
        {
            Text = "Change settings, then select Generate Preview.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 12, 0, 8)
        };

        GenerationSettings ReadSettings() => settings with
        {
            Seed = ParseInt(fields.Seed, "Seed"),
            MinPlanets = ParseInt(fields.MinPlanets, "Minimum planets"),
            MaxPlanets = ParseInt(fields.MaxPlanets, "Maximum planets"),
            MinMoonsPerPlanet = ParseInt(fields.MinMoons, "Minimum moons"),
            MaxMoonsPerPlanet = ParseInt(fields.MaxMoons, "Maximum moons"),
            MinAsteroidBelts = ParseInt(fields.MinBelts, "Minimum asteroid belts"),
            MaxAsteroidBelts = ParseInt(fields.MaxBelts, "Maximum asteroid belts"),
            FirstOrbitMinAu = ParseDouble(fields.FirstOrbitMin, "First orbit minimum"),
            FirstOrbitMaxAu = ParseDouble(fields.FirstOrbitMax, "First orbit maximum"),
            OrbitalSpacingMinMultiplier = ParseDouble(fields.SpacingMin, "Orbit spacing minimum"),
            OrbitalSpacingMaxMultiplier = ParseDouble(fields.SpacingMax, "Orbit spacing maximum"),
            MoonOrbitMinKm = ParseDouble(fields.MoonOrbitMin, "Moon orbit minimum"),
            MoonOrbitMaxKm = ParseDouble(fields.MoonOrbitMax, "Moon orbit maximum"),
            AsteroidBeltWidthMinAu = ParseDouble(fields.BeltWidthMin, "Belt width minimum"),
            AsteroidBeltWidthMaxAu = ParseDouble(fields.BeltWidthMax, "Belt width maximum"),
            AllowBinaryStars = binaryStars.IsChecked == true,
            GenerateMoons = generateMoons.IsChecked == true,
            GenerateAsteroidBelts = generateBelts.IsChecked == true,
            ShowGpsMarkers = gpsMarkers.IsChecked == true
        };

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 8, 0, 8) };
        var preview = new Button { Content = "Generate Preview" };
        preview.Click += (_, _) =>
        {
            try
            {
                var candidate = ReadSettings();
                candidate.Validate();
                result.Text = FormatPreview(Plugin.GeneratePreview(candidate));
            }
            catch (Exception ex)
            {
                result.Text = "Could not generate preview: " + ex.Message;
            }
        };

        var save = new Button { Content = "Save Settings" };
        save.Click += (_, _) =>
        {
            try
            {
                var candidate = ReadSettings();
                candidate.Validate();
                Plugin.SaveSettings(candidate);
                result.Text = "Settings saved. " + FormatPreview(Plugin.GeneratePreview(candidate));
            }
            catch (Exception ex)
            {
                result.Text = "Could not save settings: " + ex.Message;
            }
        };

        actions.Children.Add(preview);
        actions.Children.Add(save);
        host.Children.Add(actions);
        host.Children.Add(result);
    }

    private static void AddField(StackPanel host, string label, string initialValue, Action<string> assign)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Thickness(0, 3) };
        row.Children.Add(new TextBlock { Text = label, Width = 245, VerticalAlignment = VerticalAlignment.Center });
        var input = new TextBox { Text = initialValue, Width = 130 };
        input.TextChanged += (_, _) => assign(input.Text ?? string.Empty);
        assign(initialValue);
        row.Children.Add(input);
        host.Children.Add(row);
    }

    private static CheckBox AddCheck(StackPanel host, string label, bool initialValue)
    {
        var check = new CheckBox { Content = label, IsChecked = initialValue, Margin = new Thickness(0, 3) };
        host.Children.Add(check);
        return check;
    }

    private static int ParseInt(string value, string name) =>
        int.TryParse(value, out var parsed) ? parsed : throw new FormatException($"{name} must be a whole number.");

    private static double ParseDouble(string value, string name) =>
        double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new FormatException($"{name} must be a number using a period for decimals.");

    private static string FormatPreview(GeneratedStarSystem system)
    {
        var summary = $"Preview: {system.Name}\nSeed: {system.Seed}\nStars: {system.Stars.Count}\nPlanets: {system.Planets.Count}\nAsteroid belts: {system.AsteroidBelts.Count}";
        foreach (var planet in system.Planets)
            summary += $"\n• {planet.Name} — {planet.Class}, {planet.OrbitalDistanceAu:0.00} AU, {planet.Moons.Count} moons";
        return summary;
    }

    private sealed class SettingsFields
    {
        public string Seed, MinPlanets, MaxPlanets, MinMoons, MaxMoons, MinBelts, MaxBelts;
        public string FirstOrbitMin, FirstOrbitMax, SpacingMin, SpacingMax, MoonOrbitMin, MoonOrbitMax, BeltWidthMin, BeltWidthMax;
    }
}

internal sealed class PreviewSettingsScreen : ScreenView
{
    private readonly StackPanel contentHost;
    private readonly TextBlock titleText;

    public PreviewSettingsScreen()
    {
        titleText = new TextBlock
        {
            Text = "Random Star System Generator",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 12)
        };

        contentHost = new StackPanel { Spacing = 5 };
        var scroll = new ScrollViewer
        {
            Content = contentHost,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var close = new Button { Content = "Close", HorizontalAlignment = HorizontalAlignment.Right };
        close.Click += (_, _) => Dispose();

        var root = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(28),
            Width = 650,
            MaxHeight = 760,
            Background = new SolidColorBrush(Color.FromArgb(238, 30, 38, 46))
        };
        root.Children.Add(titleText);
        root.Children.Add(scroll);
        root.Children.Add(close);

        Content = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
            Child = root,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(18)
        };
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
        Background = Brushes.Transparent;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is PreviewSettingsScreenViewModel vm)
        {
            titleText.Text = vm.Title;
            contentHost.Children.Clear();
            vm.BuildContent?.Invoke(contentHost);
        }
    }
}
