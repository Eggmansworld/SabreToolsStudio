using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// Base class for all feature pages: builds the CLI argument list, keeps the
/// command preview current, and provides named preset save/load/delete.
/// </summary>
public abstract partial class FeaturePageViewModel : ViewModelBase
{
    protected readonly SettingsService _settings;
    protected readonly PresetService _presets;

    private bool _suppressPresetLoad;

    protected FeaturePageViewModel(SettingsService settings, PresetService presets)
    {
        _settings = settings;
        _presets = presets;

        PresetNames = new ObservableCollection<string>(_presets.List(FeatureKey));

        // Any option change invalidates the command preview
        PropertyChanged += OnAnyPropertyChanged;
        _settings.Changed += NotifyCommandChanged;
    }

    /// <summary>Display title of the feature page</summary>
    public abstract string Title { get; }

    /// <summary>Short description shown under the title</summary>
    public abstract string Description { get; }

    /// <summary>CLI feature name passed as the first argument. Current SabreTools main
    /// expects the bare name (e.g. "stats"), not the dashed flag form.</summary>
    public abstract string Flag { get; }

    /// <summary>Stable key used for preset storage, e.g. "stats"</summary>
    public abstract string FeatureKey { get; }

    /// <summary>Feature-specific option arguments (not including the feature flag or inputs)</summary>
    protected abstract IEnumerable<string> BuildOptionArguments();

    /// <summary>Input file/folder paths appended at the end of the command</summary>
    protected abstract IEnumerable<string> InputPaths { get; }

    public virtual bool CanRun => InputPaths.Any();

    /// <summary>Full argument list: feature flag, script mode, global settings, options, inputs</summary>
    public IReadOnlyList<string> BuildFullArguments()
    {
        var args = new List<string> { Flag, "--script" };

        AppSettings s = _settings.Current;
        if (!string.IsNullOrWhiteSpace(s.LogLevel) && s.LogLevel != "verbose")
            args.Add($"--log-level={s.LogLevel}");

        args.AddRange(BuildOptionArguments());
        args.AddRange(InputPaths);
        return args;
    }

    public string CommandPreview =>
        "SabreTools " + string.Join(' ', BuildFullArguments().Select(QuoteForDisplay));

    private static string QuoteForDisplay(string arg)
    {
        if (!arg.Contains(' ') && !arg.Contains('"'))
            return arg;

        // For key=value args, quote only the value part for CLI-correct display
        int eq = arg.IndexOf('=');
        if (arg.StartsWith('-') && eq > 0)
            return $"{arg[..(eq + 1)]}\"{arg[(eq + 1)..].Replace("\"", "\\\"")}\"";
        return $"\"{arg.Replace("\"", "\\\"")}\"";
    }

    /// <summary>Split a multiline/comma-separated text box into trimmed, non-empty values</summary>
    protected static IEnumerable<string> SplitMultiline(string text) =>
        text.Split(['\r', '\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    protected void NotifyCommandChanged()
    {
        OnPropertyChanged(nameof(CommandPreview));
        OnPropertyChanged(nameof(CanRun));
    }

    private void OnAnyPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(CommandPreview) or nameof(CanRun)))
            NotifyCommandChanged();
    }

    #region Presets

    public ObservableCollection<string> PresetNames { get; }

    /// <summary>Name typed by the user for saving a preset</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SavePresetCommand))]
    private string _presetName = "";

    /// <summary>Currently selected preset; selecting one loads it immediately</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeletePresetCommand))]
    private string? _selectedPreset;

    /// <summary>Serializable snapshot of the page's current options</summary>
    protected abstract object CapturePreset();

    /// <summary>Restore the page's options from a previously captured snapshot</summary>
    protected abstract void ApplyPreset(JsonElement data);

    partial void OnSelectedPresetChanged(string? value)
    {
        if (_suppressPresetLoad || string.IsNullOrEmpty(value))
            return;

        PresetName = value;
        if (_presets.Load(FeatureKey, value) is JsonElement data)
            ApplyPreset(data);
    }

    private bool CanSavePreset() => !string.IsNullOrWhiteSpace(PresetName);

    [RelayCommand(CanExecute = nameof(CanSavePreset))]
    private void SavePreset()
    {
        string name = PresetName.Trim();
        _presets.Save(FeatureKey, name, CapturePreset());

        if (!PresetNames.Contains(name))
        {
            PresetNames.Add(name);
            var sorted = PresetNames.Order().ToList();
            PresetNames.Clear();
            foreach (string n in sorted)
                PresetNames.Add(n);
        }

        _suppressPresetLoad = true;
        SelectedPreset = name;
        _suppressPresetLoad = false;
    }

    private bool CanDeletePreset() => !string.IsNullOrEmpty(SelectedPreset);

    [RelayCommand(CanExecute = nameof(CanDeletePreset))]
    private void DeletePreset()
    {
        if (SelectedPreset is not string name)
            return;

        _presets.Delete(FeatureKey, name);
        _suppressPresetLoad = true;
        SelectedPreset = null;
        _suppressPresetLoad = false;
        PresetNames.Remove(name);
    }

    #endregion
}
