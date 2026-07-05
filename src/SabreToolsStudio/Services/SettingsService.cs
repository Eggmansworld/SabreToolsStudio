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

    /// <summary>Optional thread count passed to all operations (--threads=)</summary>
    public int? Threads { get; set; }

    /// <summary>Lowest log level passed to all operations (--log-level=)</summary>
    public string LogLevel { get; set; } = "verbose";
}

/// <summary>
/// Loads and saves application settings as JSON in the per-user config directory
/// </summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AppSettings Current { get; private set; } = new();

    /// <summary>Raised after settings are saved so dependent state (e.g. command previews) can refresh</summary>
    public event Action? Changed;

    public static string ConfigDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SabreToolsStudio");

    private static string SettingsPath => Path.Combine(ConfigDirectory, "settings.json");

    public void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                Current = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new AppSettings();
        }
        catch
        {
            // Corrupt settings should never prevent startup; fall back to defaults
            Current = new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigDirectory);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(Current, _jsonOptions));
        }
        catch
        {
            // Non-fatal: settings just won't persist this run
        }

        Changed?.Invoke();
    }
}
