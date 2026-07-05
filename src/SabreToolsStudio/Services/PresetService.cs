using System.Text.Json;

namespace SabreToolsStudio.Services;

/// <summary>
/// Stores named option presets per feature inside the single portable
/// SabreToolsStudio.config file
/// </summary>
public sealed class PresetService(SettingsService store)
{
    private Dictionary<string, Dictionary<string, JsonElement>> Presets => store.Config.Presets;

    public IReadOnlyList<string> List(string feature) =>
        Presets.TryGetValue(feature, out var featurePresets)
            ? [.. featurePresets.Keys.Order()]
            : [];

    public void Save(string feature, string name, object data)
    {
        if (!Presets.TryGetValue(feature, out var featurePresets))
        {
            featurePresets = [];
            Presets[feature] = featurePresets;
        }

        featurePresets[name.Trim()] = JsonSerializer.SerializeToElement(data, data.GetType());
        store.Save();
    }

    public JsonElement? Load(string feature, string name) =>
        Presets.TryGetValue(feature, out var featurePresets) && featurePresets.TryGetValue(name, out JsonElement data)
            ? data
            : null;

    public void Delete(string feature, string name)
    {
        if (Presets.TryGetValue(feature, out var featurePresets) && featurePresets.Remove(name))
        {
            if (featurePresets.Count == 0)
                Presets.Remove(feature);
            store.Save();
        }
    }
}
