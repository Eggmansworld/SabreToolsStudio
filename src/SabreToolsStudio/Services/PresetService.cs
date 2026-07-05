using System.Text.Json;

namespace SabreToolsStudio.Services;

/// <summary>
/// Stores named option presets per feature as JSON files under the user config directory
/// </summary>
public sealed class PresetService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private static string FeatureDirectory(string feature) =>
        Path.Combine(SettingsService.ConfigDirectory, "presets", feature);

    private static string PresetPath(string feature, string name) =>
        Path.Combine(FeatureDirectory(feature), Sanitize(name) + ".json");

    private static string Sanitize(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim();
    }

    public IReadOnlyList<string> List(string feature)
    {
        try
        {
            string dir = FeatureDirectory(feature);
            if (!Directory.Exists(dir))
                return [];

            return [.. Directory.GetFiles(dir, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrEmpty(n))
                .Order()!];
        }
        catch
        {
            return [];
        }
    }

    public void Save(string feature, string name, object data)
    {
        Directory.CreateDirectory(FeatureDirectory(feature));
        File.WriteAllText(PresetPath(feature, name), JsonSerializer.Serialize(data, data.GetType(), _jsonOptions));
    }

    public JsonElement? Load(string feature, string name)
    {
        try
        {
            string path = PresetPath(feature, name);
            if (!File.Exists(path))
                return null;

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    public void Delete(string feature, string name)
    {
        try
        {
            string path = PresetPath(feature, name);
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Non-fatal
        }
    }
}
