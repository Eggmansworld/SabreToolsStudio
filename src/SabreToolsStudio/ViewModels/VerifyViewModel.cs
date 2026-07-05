using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// Verify from DAT feature (verify): check folders against DATs and produce a fixdat of missing files
/// </summary>
public partial class VerifyViewModel : FeaturePageViewModel
{
    public VerifyViewModel(SettingsService settings, PresetService presets)
        : base(settings, presets)
    {
        Inputs.CollectionChanged += (_, _) => NotifyCommandChanged();
        Dats.CollectionChanged += (_, _) => NotifyCommandChanged();
        MergeMode = OptionCatalog.MergeModes[0];
    }

    public override string Title => "Verify";
    public override string Description =>
        "Check input folders against one or more DAT files. Items are verified directly, and a fixdat of missing files is created afterwards in the output directory.";
    public override string Flag => "verify";
    public override string FeatureKey => "verify";

    /// <summary>Folders to verify</summary>
    public ObservableCollection<string> Inputs { get; } = [];

    /// <summary>DAT files (or folders of DATs) to verify against</summary>
    public ObservableCollection<string> Dats { get; } = [];

    [ObservableProperty]
    private string _outputDir = "";

    [ObservableProperty]
    private bool _hashOnly;

    [ObservableProperty]
    private bool _quick;

    [ObservableProperty]
    private bool _individual;

    [ObservableProperty]
    private bool _chdsAsFiles;

    [ObservableProperty]
    private bool _aaruFormatsAsFiles;

    [ObservableProperty]
    private bool _depot;

    [ObservableProperty]
    private string _depotDepth = "";

    [ObservableProperty]
    private bool _useHeaderSkipper;

    [ObservableProperty]
    private string _headerSkipper = "";

    public IReadOnlyList<ComboOption> MergeModes => OptionCatalog.MergeModes;

    [ObservableProperty]
    private ComboOption _mergeMode;

    [ObservableProperty]
    private string _filters = "";

    [ObservableProperty]
    private bool _matchOfTags;

    [ObservableProperty]
    private string _extraInis = "";

    /// <summary>Verification needs both inputs and at least one DAT</summary>
    public override bool CanRun => Inputs.Count > 0 && Dats.Count > 0;

    protected override IEnumerable<string> InputPaths => Inputs;

    protected override IEnumerable<string> BuildOptionArguments()
    {
        foreach (string dat in Dats)
            yield return $"--dat={dat}";

        if (!string.IsNullOrWhiteSpace(OutputDir))
            yield return $"--output-dir={OutputDir.Trim()}";

        if (HashOnly)
            yield return "--hash-only";
        if (Quick)
            yield return "--quick";
        if (Individual)
            yield return "--individual";
        if (ChdsAsFiles)
            yield return "--chds-as-files";
        if (AaruFormatsAsFiles)
            yield return "--aaruformats-as-files";

        if (Depot)
        {
            yield return "--depot";
            if (int.TryParse(DepotDepth, out int depth) && depth >= 0)
                yield return $"--depot-depth={depth}";
        }

        if (UseHeaderSkipper)
            yield return $"--header={HeaderSkipper.Trim()}";

        if (MergeMode.Value.Length > 0)
            yield return MergeMode.Value;

        bool hasFilters = false;
        foreach (string filter in SplitMultiline(Filters))
        {
            hasFilters = true;
            yield return $"--filter={filter}";
        }

        if (MatchOfTags && hasFilters)
            yield return "--match-of-tags";

        foreach (string ini in SplitMultiline(ExtraInis))
            yield return $"--extra-ini={ini}";
    }

    #region Presets

    private sealed class VerifyPreset
    {
        public List<string> Inputs { get; set; } = [];
        public List<string> Dats { get; set; } = [];
        public string OutputDir { get; set; } = "";
        public bool HashOnly { get; set; }
        public bool Quick { get; set; }
        public bool Individual { get; set; }
        public bool ChdsAsFiles { get; set; }
        public bool AaruFormatsAsFiles { get; set; }
        public bool Depot { get; set; }
        public string DepotDepth { get; set; } = "";
        public bool UseHeaderSkipper { get; set; }
        public string HeaderSkipper { get; set; } = "";
        public string MergeMode { get; set; } = "";
        public string Filters { get; set; } = "";
        public bool MatchOfTags { get; set; }
        public string ExtraInis { get; set; } = "";
    }

    protected override object CapturePreset() => new VerifyPreset
    {
        Inputs = [.. Inputs],
        Dats = [.. Dats],
        OutputDir = OutputDir,
        HashOnly = HashOnly,
        Quick = Quick,
        Individual = Individual,
        ChdsAsFiles = ChdsAsFiles,
        AaruFormatsAsFiles = AaruFormatsAsFiles,
        Depot = Depot,
        DepotDepth = DepotDepth,
        UseHeaderSkipper = UseHeaderSkipper,
        HeaderSkipper = HeaderSkipper,
        MergeMode = MergeMode.Value,
        Filters = Filters,
        MatchOfTags = MatchOfTags,
        ExtraInis = ExtraInis,
    };

    protected override void ApplyPreset(JsonElement data)
    {
        VerifyPreset? preset;
        try
        {
            preset = data.Deserialize<VerifyPreset>();
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
        HashOnly = preset.HashOnly;
        Quick = preset.Quick;
        Individual = preset.Individual;
        ChdsAsFiles = preset.ChdsAsFiles;
        AaruFormatsAsFiles = preset.AaruFormatsAsFiles;
        Depot = preset.Depot;
        DepotDepth = preset.DepotDepth;
        UseHeaderSkipper = preset.UseHeaderSkipper;
        HeaderSkipper = preset.HeaderSkipper;
        MergeMode = MergeModes.FirstOrDefault(m => m.Value == preset.MergeMode) ?? MergeModes[0];
        Filters = preset.Filters;
        MatchOfTags = preset.MatchOfTags;
        ExtraInis = preset.ExtraInis;
    }

    #endregion
}
