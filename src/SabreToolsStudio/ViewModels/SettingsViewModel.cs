using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// Application settings page: theme, SabreTools executable, and global CLI options
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settings;
    private readonly SabreToolsLocator _locator;
    private bool _loading = true;

    public SettingsViewModel(SettingsService settings, SabreToolsLocator locator)
    {
        _settings = settings;
        _locator = locator;

        SelectedTheme = settings.Current.Theme is "Light" or "Dark" ? settings.Current.Theme : "System";
        SabreToolsPath = settings.Current.SabreToolsPath ?? "";
        SelectedLogLevel = LogLevels.Contains(settings.Current.LogLevel) ? settings.Current.LogLevel : "verbose";
        _loading = false;
    }

    public string Title => "Settings";

    public static string[] ThemeOptions { get; } = ["System", "Light", "Dark"];
    public static string[] LogLevels { get; } = ["verbose", "user", "warning", "error"];

    [ObservableProperty]
    private string _selectedTheme = "System";

    [ObservableProperty]
    private string _sabreToolsPath = "";

    [ObservableProperty]
    private string _selectedLogLevel = "verbose";

    /// <summary>The executable that will actually be used, for display</summary>
    public string ResolvedPathDisplay =>
        _locator.Resolve() ?? "No SabreTools executable found - set a path above or reinstall.";

    /// <summary>Where all settings and presets are stored, for display</summary>
    public static string ConfigPathDisplay => SettingsService.ConfigPath;

    public static void ApplyTheme(string theme)
    {
        if (Application.Current is not null)
        {
            Application.Current.RequestedThemeVariant = theme switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default,
            };
        }
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (_loading)
            return;

        ApplyTheme(value);
        _settings.Current.Theme = value;
        _settings.Save();
    }

    partial void OnSabreToolsPathChanged(string value)
    {
        if (_loading)
            return;

        _settings.Current.SabreToolsPath = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        _settings.Save();
        OnPropertyChanged(nameof(ResolvedPathDisplay));
    }

    partial void OnSelectedLogLevelChanged(string value)
    {
        if (_loading)
            return;

        _settings.Current.LogLevel = value;
        _settings.Save();
    }

    /// <summary>Set from the view's file picker</summary>
    public void SetExecutablePath(string path) => SabreToolsPath = path;

    [RelayCommand]
    private void UseBundled() => SabreToolsPath = "";
}
