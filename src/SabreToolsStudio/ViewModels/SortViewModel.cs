using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// Sort/Rebuild feature (sort): rebuild input files into organized sets based on DATs
/// </summary>
public partial class SortViewModel : FeaturePageViewModel
{
    public SortViewModel(SettingsService settings, PresetService presets)
        : base(settings, presets)
    {
        Inputs.CollectionChanged += (_, _) => NotifyCommandChanged();
        Dats.CollectionChanged += (_, _) => NotifyCommandChanged();
        RebuildFormat = OptionCatalog.RebuildFormats[0];
        MergeMode = OptionCatalog.MergeModes[0];
    }

    public override string Title => "Sort / Rebuild";
    public override string Description =>
        "Rebuild files into organized sets based on one or more DAT files. By default all matched files are rebuilt to uncompressed folders in the output directory.";
    public override string Flag => "sort";
    public override string FeatureKey => "sort";

    /// <summary>Folders or files to rebuild from</summary>
    public ObservableCollection<string> Inputs { get; } = [];

    /// <summary>DAT files (or folders of DATs) describing the desired sets</summary>
    public ObservableCollection<string> Dats { get; } = [];

    [ObservableProperty]
    private string _outputDir = "";

    public IReadOnlyList<ComboOption> RebuildFormats => OptionCatalog.RebuildFormats;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTorrentGzipSelected))]
    private ComboOption _rebuildFormat;

    /// <summary>True when TorrentGZ output is selected, enabling the Romba depot option</summary>
    public bool IsTorrentGzipSelected => RebuildFormat.Value == "--torrent-gzip";

    [ObservableProperty]
    private bool _romba;

    [ObservableProperty]
    private string _rombaDepth = "";

    [ObservableProperty]
    private bool _depot;

    [ObservableProperty]
    private string _depotDepth = "";

    [ObservableProperty]
    private bool _quick;

    [ObservableProperty]
    private bool _delete;

    [ObservableProperty]
    private bool _inverse;

    [ObservableProperty]
    private bool _addDate;

    [ObservableProperty]
    private bool _individual;

    [ObservableProperty]
    private bool _chdsAsFiles;

    [ObservableProperty]
    private bool _aaruFormatsAsFiles;

    [ObservableProperty]
    private bool _useHeaderSkipper;

    [ObservableProperty]
    private string _headerSkipper = "";

    public IReadOnlyList<ComboOption> MergeModes => OptionCatalog.MergeModes;

    [ObservableProperty]
    private ComboOption _mergeMode;

    [ObservableProperty]
    private bool _updateDat;

    /// <summary>Rebuilding needs both inputs and at least one DAT</summary>
    public override bool CanRun => Inputs.Count > 0 && Dats.Count > 0;

    protected override IEnumerable<string> InputPaths => Inputs;

    protected override IEnumerable<string> BuildOptionArguments()
    {
        foreach (string dat in Dats)
            yield return $"--dat={dat}";

        if (!string.IsNullOrWhiteSpace(OutputDir))
            yield return $"--output-dir={OutputDir.Trim()}";

        if (RebuildFormat.Value.Length > 0)
            yield return RebuildFormat.Value;

        if (Romba && IsTorrentGzipSelected)
        {
            yield return "--romba";
            if (int.TryParse(RombaDepth, out int rombaDepth) && rombaDepth >= 0)
                yield return $"--romba-depth={rombaDepth}";
        }

        if (Depot)
        {
            yield return "--depot";
            if (int.TryParse(DepotDepth, out int depotDepth) && depotDepth >= 0)
                yield return $"--depot-depth={depotDepth}";
        }

        if (Quick)
            yield return "--quick";
        if (Delete)
            yield return "--delete";
        if (Inverse)
            yield return "--inverse";
        if (AddDate)
            yield return "--add-date";
        if (Individual)
            yield return "--individual";
        if (ChdsAsFiles)
            yield return "--chds-as-files";
        if (AaruFormatsAsFiles)
            yield return "--aaruformats-as-files";

        if (UseHeaderSkipper)
            yield return $"--header={HeaderSkipper.Trim()}";

        if (MergeMode.Value.Length > 0)
            yield return MergeMode.Value;

        if (UpdateDat)
            yield return "--update-dat";
    }

    #region Presets

    private sealed class SortPreset
    {
        public List<string> Inputs { get; set; } = [];
        public List<string> Dats { get; set; } = [];
        public string OutputDir { get; set; } = "";
        public string RebuildFormat { get; set; } = "";
        public bool Romba { get; set; }
        public string RombaDepth { get; set; } = "";
        public bool Depot { get; set; }
        public string DepotDepth { get; set; } = "";
        public bool Quick { get; set; }
        public bool Delete { get; set; }
        public bool Inverse { get; set; }
        public bool AddDate { get; set; }
        public bool Individual { get; set; }
        public bool ChdsAsFiles { get; set; }
        public bool AaruFormatsAsFiles { get; set; }
        public bool UseHeaderSkipper { get; set; }
        public string HeaderSkipper { get; set; } = "";
        public string MergeMode { get; set; } = "";
        public bool UpdateDat { get; set; }
    }

    protected override object CapturePreset() => new SortPreset
    {
        Inputs = [.. Inputs],
        Dats = [.. Dats],
        OutputDir = OutputDir,
        RebuildFormat = RebuildFormat.Value,
        Romba = Romba,
        RombaDepth = RombaDepth,
        Depot = Depot,
        DepotDepth = DepotDepth,
        Quick = Quick,
        Delete = Delete,
        Inverse = Inverse,
        AddDate = AddDate,
        Individual = Individual,
        ChdsAsFiles = ChdsAsFiles,
        AaruFormatsAsFiles = AaruFormatsAsFiles,
        UseHeaderSkipper = UseHeaderSkipper,
        HeaderSkipper = HeaderSkipper,
        MergeMode = MergeMode.Value,
        UpdateDat = UpdateDat,
    };

    protected override void ApplyPreset(JsonElement data)
    {
        SortPreset? preset;
        try
        {
            preset = data.Deserialize<SortPreset>();
        }
        catch
        {
            return;
        }

        if (preset is null)
            return;

        Inputs.Clear();
        foreach (string input in preset.Inputs.Where(p => !string.IsNullOrWhiteSpace(p)))
            Inputs.Add(input);

        Dats.Clear();
        foreach (string dat in preset.Dats.Where(p => !string.IsNullOrWhiteSpace(p)))
            Dats.Add(dat);

        OutputDir = preset.OutputDir;
        RebuildFormat = RebuildFormats.FirstOrDefault(f => f.Value == preset.RebuildFormat) ?? RebuildFormats[0];
        Romba = preset.Romba;
        RombaDepth = preset.RombaDepth;
        Depot = preset.Depot;
        DepotDepth = preset.DepotDepth;
        Quick = preset.Quick;
        Delete = preset.Delete;
        Inverse = preset.Inverse;
        AddDate = preset.AddDate;
        Individual = preset.Individual;
        ChdsAsFiles = preset.ChdsAsFiles;
        AaruFormatsAsFiles = preset.AaruFormatsAsFiles;
        UseHeaderSkipper = preset.UseHeaderSkipper;
        HeaderSkipper = preset.HeaderSkipper;
        MergeMode = MergeModes.FirstOrDefault(m => m.Value == preset.MergeMode) ?? MergeModes[0];
        UpdateDat = preset.UpdateDat;
    }

    #endregion
}
