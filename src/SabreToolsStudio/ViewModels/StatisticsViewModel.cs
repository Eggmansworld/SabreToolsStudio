using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// Statistics Output feature (--stats): combined or individual statistics for input DAT files
/// </summary>
public partial class StatisticsViewModel : FeaturePageViewModel
{
    public StatisticsViewModel(SettingsService settings, PresetService presets)
        : base(settings, presets)
    {
        ReportFormats =
        [
            new("text", "Text", "Generic text file report. This is the default format if none is selected."),
            new("csv", "CSV", "Standardized comma-separated value report, suitable for spreadsheets."),
            new("html", "HTML", "HTML webpage report, viewable in any browser."),
            new("ssv", "SSV", "Standardized semicolon-separated value report."),
            new("tsv", "TSV", "Standardized tab-separated value report."),
        ];

        Inputs.CollectionChanged += OnInputsChanged;
        foreach (FormatOption format in ReportFormats)
            format.PropertyChanged += (_, _) => NotifyCommandChanged();
    }

    public override string Title => "Statistics";
    public override string Description =>
        "Output combined statistics for input DAT files: game/rom/disk counts, total uncompressed size, and hash coverage.";
    public override string Flag => "stats";
    public override string FeatureKey => "stats";

    /// <summary>Input DAT files or folders of DATs</summary>
    public ObservableCollection<string> Inputs { get; } = [];

    public ObservableCollection<FormatOption> ReportFormats { get; }

    [ObservableProperty]
    private string _filename = "";

    [ObservableProperty]
    private string _outputDir = "";

    [ObservableProperty]
    private bool _baddumpColumn;

    [ObservableProperty]
    private bool _nodumpColumn;

    [ObservableProperty]
    private bool _individual;

    private void OnInputsChanged(object? sender, NotifyCollectionChangedEventArgs e) => NotifyCommandChanged();

    protected override IEnumerable<string> InputPaths => Inputs;

    protected override IEnumerable<string> BuildOptionArguments()
    {
        foreach (FormatOption format in ReportFormats)
        {
            if (format.IsSelected)
                yield return $"--report-type={format.Value}";
        }

        if (!string.IsNullOrWhiteSpace(Filename))
            yield return $"--filename={Filename.Trim()}";
        if (!string.IsNullOrWhiteSpace(OutputDir))
            yield return $"--output-dir={OutputDir.Trim()}";

        if (BaddumpColumn)
            yield return "--baddump-column";
        if (NodumpColumn)
            yield return "--nodump-column";
        if (Individual)
            yield return "--individual";
    }

    #region Presets

    private sealed class StatisticsPreset
    {
        public List<string> Formats { get; set; } = [];
        public string Filename { get; set; } = "";
        public string OutputDir { get; set; } = "";
        public bool BaddumpColumn { get; set; }
        public bool NodumpColumn { get; set; }
        public bool Individual { get; set; }
        public List<string> Inputs { get; set; } = [];
    }

    protected override object CapturePreset() => new StatisticsPreset
    {
        Formats = [.. ReportFormats.Where(f => f.IsSelected).Select(f => f.Value)],
        Filename = Filename,
        OutputDir = OutputDir,
        BaddumpColumn = BaddumpColumn,
        NodumpColumn = NodumpColumn,
        Individual = Individual,
        Inputs = [.. Inputs],
    };

    protected override void ApplyPreset(JsonElement data)
    {
        StatisticsPreset? preset;
        try
        {
            preset = data.Deserialize<StatisticsPreset>();
        }
        catch
        {
            return;
        }

        if (preset is null)
            return;

        foreach (FormatOption format in ReportFormats)
            format.IsSelected = preset.Formats.Contains(format.Value);

        Filename = preset.Filename;
        OutputDir = preset.OutputDir;
        BaddumpColumn = preset.BaddumpColumn;
        NodumpColumn = preset.NodumpColumn;
        Individual = preset.Individual;

        Inputs.Clear();
        foreach (string input in preset.Inputs.Where(p => !string.IsNullOrWhiteSpace(p)))
            Inputs.Add(input);
    }

    #endregion
}
