using System.Text.Json;

namespace SabreToolsStudio.Services;

/// <summary>
/// Persisted application settings
/// </summary>
public sealed class AppSettings
{
    /// <summary>Theme selection: "System", "Light", or "Dark"</summary>
    public string Theme { get; set; } = "System";

    /// <summary>Optional user override for the SabreTools executable path; null uses the bundled copy</summary>
    public string? SabreToolsPath { get; set; }

    /// <summary>Lowest log level passed to all operations (--log-level=)</summary>
    public string LogLevel { get; set; } = "verbose";
}

/// <summary>
/// Everything persisted by the application: settings plus all named presets,
/// stored as a single portable file
/// </summary>
public sealed class AppConfig
{
    public AppSettings Settings { get; set; } = new();

    /// <summary>Presets by feature key, then by preset name</summary>
    public Dictionary<string, Dictionary<string, JsonElement>> Presets { get; set; } = [];
}

/// <summary>
/// Loads and saves the single SabreToolsStudio.config file kept next to the
/// executable, so the application is fully portable: no registry, no user
/// profile folders, no system temp files.
/// </summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AppConfig Config { get; private set; } = new();

    public AppSettings Current => Config.Settings;

    /// <summary>Raised after the config is saved so dependent state (e.g. command previews) can refresh</summary>
    public event Action? Changed;

    /// <summary>The single portable config file, next to SabreToolsStudio.exe</summary>
    public static string ConfigPath =>
        Path.Combine(AppContext.BaseDirectory, "SabreToolsStudio.config");

    public void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                Config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new AppConfig();
                return;
            }

            MigrateLegacySettings();
        }
        catch
        {
            // A corrupt config should never prevent startup; fall back to defaults
            Config = new AppConfig();
        }
    }

    /// <summary>
    /// Import settings from the pre-portable location (%APPDATA%/SabreToolsStudio)
    /// and remove that folder so nothing is left behind in the user profile
    /// </summary>
    private void MigrateLegacySettings()
    {
        try
        {
            string legacyDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SabreToolsStudio");
            string legacySettings = Path.Combine(legacyDir, "settings.json");

            if (File.Exists(legacySettings))
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(legacySettings));
                if (settings is not null)
                {
                    Config.Settings = settings;
                    Save();
                }
            }

            if (Directory.Exists(legacyDir))
                Directory.Delete(legacyDir, recursive: true);
        }
        catch
        {
            // Migration is best-effort only
        }
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Config, _jsonOptions));
        }
        catch
        {
            // Non-fatal: settings just won't persist this run (e.g. read-only location)
        }

        Changed?.Invoke();
    }
}
