using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// DAT From Dir feature (d2d): create DAT files from input directories
/// </summary>
public partial class DatFromDirViewModel : FeaturePageViewModel
{
    public DatFromDirViewModel(SettingsService settings, PresetService presets)
        : base(settings, presets)
    {
        HashOptions = new ObservableCollection<FormatOption>(OptionCatalog.CreateHashOptions());
        OutputFormats = new ObservableCollection<FormatOption>(OptionCatalog.CreateDatFormats());

        Inputs.CollectionChanged += (_, _) => NotifyCommandChanged();
        foreach (FormatOption hash in HashOptions)
            hash.PropertyChanged += (_, _) => NotifyCommandChanged();
        foreach (FormatOption format in OutputFormats)
            format.PropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsXmlSelected));
                NotifyCommandChanged();
            };

        ForceMerging = OptionCatalog.ForceMergingModes[0];
        ForceNodump = OptionCatalog.ForceNodumpModes[0];
        ForcePacking = OptionCatalog.ForcePackingModes[0];
    }

    public override string Title => "DAT From Dir";
    public override string Description =>
        "Create a DAT file from an input directory or set of files. By default the DAT is named after the input directory and the current date, and CRC-32, MD5, and SHA-1 are calculated for every file.";
    public override string Flag => "d2d";
    public override string FeatureKey => "d2d";

    #region Setup options

    /// <summary>Input directories or files to scan</summary>
    public ObservableCollection<string> Inputs { get; } = [];

    /// <summary>Hashes to include; none selected means the CLI default of CRC-32 + MD5 + SHA-1</summary>
    public ObservableCollection<FormatOption> HashOptions { get; }

    /// <summary>Output DAT formats; none selected means the CLI default of Logiqx XML</summary>
    public ObservableCollection<FormatOption> OutputFormats { get; }

    /// <summary>True when the Logiqx XML format is selected, enabling the deprecated-game-tag option</summary>
    public bool IsXmlSelected => OutputFormats.Any(f => f is { Value: "xml", IsSelected: true });

    [ObservableProperty]
    private string _outputDir = "";

    [ObservableProperty]
    private bool _noAutomaticDate;

    [ObservableProperty]
    private bool _deprecated;

    [ObservableProperty]
    private bool _romba;

    [ObservableProperty]
    private string _rombaDepth = "";

    [ObservableProperty]
    private bool _archivesAsFiles;

    [ObservableProperty]
    private bool _chdsAsFiles;

    [ObservableProperty]
    private bool _aaruFormatsAsFiles;

    [ObservableProperty]
    private bool _skipArchives;

    [ObservableProperty]
    private bool _skipFiles;

    [ObservableProperty]
    private bool _addBlankFiles;

    [ObservableProperty]
    private bool _addDate;

    [ObservableProperty]
    private bool _useHeaderSkipper;

    [ObservableProperty]
    private string _headerSkipper = "";

    #endregion

    #region DAT header fields

    [ObservableProperty]
    private string _datFilename = "";

    [ObservableProperty]
    private string _datName = "";

    [ObservableProperty]
    private string _datDescription = "";

    [ObservableProperty]
    private string _datCategory = "";

    [ObservableProperty]
    private string _datVersion = "";

    [ObservableProperty]
    private string _datDate = "";

    [ObservableProperty]
    private string _datAuthor = "";

    [ObservableProperty]
    private string _datEmail = "";

    [ObservableProperty]
    private string _datHomepage = "";

    [ObservableProperty]
    private string _datUrl = "";

    [ObservableProperty]
    private string _datComment = "";

    [ObservableProperty]
    private string _datRoot = "";

    [ObservableProperty]
    private bool _superDat;

    public IReadOnlyList<ComboOption> ForceMergingModes => OptionCatalog.ForceMergingModes;
    public IReadOnlyList<ComboOption> ForceNodumpModes => OptionCatalog.ForceNodumpModes;
    public IReadOnlyList<ComboOption> ForcePackingModes => OptionCatalog.ForcePackingModes;

    [ObservableProperty]
    private ComboOption _forceMerging;

    [ObservableProperty]
    private ComboOption _forceNodump;

    [ObservableProperty]
    private ComboOption _forcePacking;

    #endregion

    #region Filtering options

    [ObservableProperty]
    private bool _oneGamePerRegion;

    [ObservableProperty]
    private string _regions = "";

    [ObservableProperty]
    private bool _oneRomPerGame;

    [ObservableProperty]
    private bool _sceneDateStrip;

    [ObservableProperty]
    private string _excludeFields = "";

    [ObservableProperty]
    private string _filters = "";

    [ObservableProperty]
    private bool _matchOfTags;

    [ObservableProperty]
    private string _extraInis = "";

    #endregion

    protected override IEnumerable<string> InputPaths => Inputs;

    protected override IEnumerable<string> BuildOptionArguments()
    {
        foreach (FormatOption hash in HashOptions)
        {
            if (hash.IsSelected)
                yield return hash.Value;
        }

        foreach (FormatOption format in OutputFormats)
        {
            if (format.IsSelected)
                yield return $"--output-type={format.Value}";
        }

        if (Deprecated && IsXmlSelected)
            yield return "--deprecated";

        if (NoAutomaticDate)
            yield return "--no-automatic-date";
        if (ArchivesAsFiles)
            yield return "--archives-as-files";
        if (ChdsAsFiles)
            yield return "--chds-as-files";
        if (AaruFormatsAsFiles)
            yield return "--aaruformats-as-files";
        if (SkipArchives)
            yield return "--skip-archives";
        if (SkipFiles)
            yield return "--skip-files";
        if (AddBlankFiles)
            yield return "--add-blank-files";
        if (AddDate)
            yield return "--add-date";

        if (Romba)
        {
            yield return "--romba";
            if (int.TryParse(RombaDepth, out int depth) && depth >= 0)
                yield return $"--romba-depth={depth}";
        }

        if (UseHeaderSkipper)
            yield return $"--header={HeaderSkipper.Trim()}";

        // DAT header fields
        if (!string.IsNullOrWhiteSpace(DatFilename))
            yield return $"--filename={DatFilename.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatName))
            yield return $"--name={DatName.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatDescription))
            yield return $"--description={DatDescription.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatCategory))
            yield return $"--category={DatCategory.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatVersion))
            yield return $"--version={DatVersion.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatDate))
            yield return $"--date={DatDate.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatAuthor))
            yield return $"--author={DatAuthor.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatEmail))
            yield return $"--email={DatEmail.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatHomepage))
            yield return $"--homepage={DatHomepage.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatUrl))
            yield return $"--url={DatUrl.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatComment))
            yield return $"--comment={DatComment.Trim()}";
        if (!string.IsNullOrWhiteSpace(DatRoot))
            yield return $"--root={DatRoot.Trim()}";
        if (SuperDat)
            yield return "--superdat";

        if (ForceMerging.Value.Length > 0)
            yield return $"--forcemerging={ForceMerging.Value}";
        if (ForceNodump.Value.Length > 0)
            yield return $"--forcenodump={ForceNodump.Value}";
        if (ForcePacking.Value.Length > 0)
            yield return $"--forcepacking={ForcePacking.Value}";

        // Filtering
        if (OneGamePerRegion)
        {
            yield return "--one-game-per-region";
            foreach (string region in SplitMultiline(Regions))
                yield return $"--region={region}";
        }

        if (OneRomPerGame)
            yield return "--one-rom-per-game";
        if (SceneDateStrip)
            yield return "--scene-date-strip";

        foreach (string field in SplitMultiline(ExcludeFields))
            yield return $"--exclude-field={field}";

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

        if (!string.IsNullOrWhiteSpace(OutputDir))
            yield return $"--output-dir={OutputDir.Trim()}";
    }

    #region Presets

    private sealed class DatFromDirPreset
    {
        public List<string> Inputs { get; set; } = [];
        public List<string> Hashes { get; set; } = [];
        public List<string> Formats { get; set; } = [];
        public string OutputDir { get; set; } = "";
        public bool NoAutomaticDate { get; set; }
        public bool Deprecated { get; set; }
        public bool Romba { get; set; }
        public string RombaDepth { get; set; } = "";
        public bool ArchivesAsFiles { get; set; }
        public bool ChdsAsFiles { get; set; }
        public bool AaruFormatsAsFiles { get; set; }
        public bool SkipArchives { get; set; }
        public bool SkipFiles { get; set; }
        public bool AddBlankFiles { get; set; }
        public bool AddDate { get; set; }
        public bool UseHeaderSkipper { get; set; }
        public string HeaderSkipper { get; set; } = "";
        public Dictionary<string, string> HeaderFields { get; set; } = [];
        public bool SuperDat { get; set; }
        public string ForceMerging { get; set; } = "";
        public string ForceNodump { get; set; } = "";
        public string ForcePacking { get; set; } = "";
        public bool OneGamePerRegion { get; set; }
        public string Regions { get; set; } = "";
        public bool OneRomPerGame { get; set; }
        public bool SceneDateStrip { get; set; }
        public string ExcludeFields { get; set; } = "";
        public string Filters { get; set; } = "";
        public bool MatchOfTags { get; set; }
        public string ExtraInis { get; set; } = "";
    }

    protected override object CapturePreset() => new DatFromDirPreset
    {
        Inputs = [.. Inputs],
        Hashes = [.. HashOptions.Where(h => h.IsSelected).Select(h => h.Value)],
        Formats = [.. OutputFormats.Where(f => f.IsSelected).Select(f => f.Value)],
        OutputDir = OutputDir,
        NoAutomaticDate = NoAutomaticDate,
        Deprecated = Deprecated,
        Romba = Romba,
        RombaDepth = RombaDepth,
        ArchivesAsFiles = ArchivesAsFiles,
        ChdsAsFiles = ChdsAsFiles,
        AaruFormatsAsFiles = AaruFormatsAsFiles,
        SkipArchives = SkipArchives,
        SkipFiles = SkipFiles,
        AddBlankFiles = AddBlankFiles,
        AddDate = AddDate,
        UseHeaderSkipper = UseHeaderSkipper,
        HeaderSkipper = HeaderSkipper,
        HeaderFields = new Dictionary<string, string>
        {
            ["filename"] = DatFilename,
            ["name"] = DatName,
            ["description"] = DatDescription,
            ["category"] = DatCategory,
            ["version"] = DatVersion,
            ["date"] = DatDate,
            ["author"] = DatAuthor,
            ["email"] = DatEmail,
            ["homepage"] = DatHomepage,
            ["url"] = DatUrl,
            ["comment"] = DatComment,
            ["root"] = DatRoot,
        },
        SuperDat = SuperDat,
        ForceMerging = ForceMerging.Value,
        ForceNodump = ForceNodump.Value,
        ForcePacking = ForcePacking.Value,
        OneGamePerRegion = OneGamePerRegion,
        Regions = Regions,
        OneRomPerGame = OneRomPerGame,
        SceneDateStrip = SceneDateStrip,
        ExcludeFields = ExcludeFields,
        Filters = Filters,
        MatchOfTags = MatchOfTags,
        ExtraInis = ExtraInis,
    };

    protected override void ApplyPreset(JsonElement data)
    {
        DatFromDirPreset? preset;
        try
        {
            preset = data.Deserialize<DatFromDirPreset>();
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

        foreach (FormatOption hash in HashOptions)
            hash.IsSelected = preset.Hashes.Contains(hash.Value);
        foreach (FormatOption format in OutputFormats)
            format.IsSelected = preset.Formats.Contains(format.Value);

        OutputDir = preset.OutputDir;
        NoAutomaticDate = preset.NoAutomaticDate;
        Deprecated = preset.Deprecated;
        Romba = preset.Romba;
        RombaDepth = preset.RombaDepth;
        ArchivesAsFiles = preset.ArchivesAsFiles;
        ChdsAsFiles = preset.ChdsAsFiles;
        AaruFormatsAsFiles = preset.AaruFormatsAsFiles;
        SkipArchives = preset.SkipArchives;
        SkipFiles = preset.SkipFiles;
        AddBlankFiles = preset.AddBlankFiles;
        AddDate = preset.AddDate;
        UseHeaderSkipper = preset.UseHeaderSkipper;
        HeaderSkipper = preset.HeaderSkipper;

        DatFilename = preset.HeaderFields.GetValueOrDefault("filename", "");
        DatName = preset.HeaderFields.GetValueOrDefault("name", "");
        DatDescription = preset.HeaderFields.GetValueOrDefault("description", "");
        DatCategory = preset.HeaderFields.GetValueOrDefault("category", "");
        DatVersion = preset.HeaderFields.GetValueOrDefault("version", "");
        DatDate = preset.HeaderFields.GetValueOrDefault("date", "");
        DatAuthor = preset.HeaderFields.GetValueOrDefault("author", "");
        DatEmail = preset.HeaderFields.GetValueOrDefault("email", "");
        DatHomepage = preset.HeaderFields.GetValueOrDefault("homepage", "");
        DatUrl = preset.HeaderFields.GetValueOrDefault("url", "");
        DatComment = preset.HeaderFields.GetValueOrDefault("comment", "");
        DatRoot = preset.HeaderFields.GetValueOrDefault("root", "");

        SuperDat = preset.SuperDat;
        ForceMerging = ForceMergingModes.FirstOrDefault(m => m.Value == preset.ForceMerging) ?? ForceMergingModes[0];
        ForceNodump = ForceNodumpModes.FirstOrDefault(m => m.Value == preset.ForceNodump) ?? ForceNodumpModes[0];
        ForcePacking = ForcePackingModes.FirstOrDefault(m => m.Value == preset.ForcePacking) ?? ForcePackingModes[0];

        OneGamePerRegion = preset.OneGamePerRegion;
        Regions = preset.Regions;
        OneRomPerGame = preset.OneRomPerGame;
        SceneDateStrip = preset.SceneDateStrip;
        ExcludeFields = preset.ExcludeFields;
        Filters = preset.Filters;
        MatchOfTags = preset.MatchOfTags;
        ExtraInis = preset.ExtraInis;
    }

    #endregion
}
