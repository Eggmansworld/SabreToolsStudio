using System.Runtime.InteropServices;

namespace SabreToolsStudio.Services;

/// <summary>
/// Resolves the SabreTools executable: user override first, then the bundled copy
/// </summary>
public sealed class SabreToolsLocator(SettingsService settings)
{
    private static string ExeName =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "SabreTools.exe" : "SabreTools";

    /// <summary>Full path to the bundled CLI shipped alongside the GUI</summary>
    public static string BundledPath =>
        Path.Combine(AppContext.BaseDirectory, "sabretools", ExeName);

    /// <summary>
    /// Returns the executable path to use, or null if none can be found
    /// </summary>
    public string? Resolve()
    {
        string? overridePath = settings.Current.SabreToolsPath;
        if (!string.IsNullOrWhiteSpace(overridePath) && File.Exists(overridePath))
            return overridePath;

        return File.Exists(BundledPath) ? BundledPath : null;
    }
}
