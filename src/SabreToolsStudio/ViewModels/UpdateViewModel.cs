using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// Update and Manipulate DATs feature (update): the multitool for converting,
/// merging, diffing, filtering, and rewriting DAT files
/// </summary>
public partial class UpdateViewModel : FeaturePageViewModel
{
    public UpdateViewModel(SettingsService settings, PresetService presets)
        : base(settings, presets)
    {
        OutputFormats = new ObservableCollection<FormatOption>(OptionCatalog.CreateDatFormats());

        Inputs.CollectionChanged += (_, _) => NotifyCommandChanged();
        BaseDats.CollectionChanged += (_, _) => NotifyCommandChanged();
        foreach (FormatOption format in OutputFormats)
            format.PropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsXmlSelected));
                OnPropertyChanged(nameof(HasFormatSelected));
                NotifyCommandChanged();
            };

        ForceMerging = OptionCatalog.ForceMergingModes[0];
        ForceNodump = OptionCatalog.ForceNodumpModes[0];
        ForcePacking = OptionCatalog.ForcePackingModes[0];
        MergeMode = OptionCatalog.MergeModes[0];
        DedupMode = OptionCatalog.DedupModes[0];
        CascadeMode = OptionCatalog.CascadeModes[0];
        BaseReplaceMode = OptionCatalog.BaseReplaceModes[0];
    }

    public override string Title => "Update DATs";
    public override string Description =>
        "The multitool: convert, merge, diff, filter, dedupe, and rewrite DAT files. All options apply to every input DAT unless a combining mode is used.";
    public override string Flag => "update";
    public override string FeatureKey => "update";

    /// <summary>Input DAT files or folders of DATs</summary>
    public ObservableCollection<string> Inputs { get; } = [];

    #region Output options

    public ObservableCollection<FormatOption> OutputFormats { get; }

    public bool IsXmlSelected => OutputFormats.Any(f => f is { Value: "xml", IsSelected: true });

    /// <summary>True when at least one output format is selected, enabling the naming options</summary>
    public bool HasFormatSelected => OutputFormats.Any(f => f.IsSelected);

    [ObservableProperty]
    private bool _deprecated;

    [ObservableProperty]
    private bool _romba;

    [ObservableProperty]
    private string _rombaDepth = "";

    [ObservableProperty]
    private string _prefix = "";

    [ObservableProperty]
    private string _postfix = "";

    [ObservableProperty]
    private bool _quotes;

    [ObservableProperty]
    private bool _romsInMiss;

    [ObservableProperty]
    private bool _gamePrefix;

    [ObservableProperty]
    private string _addExtension = "";

    [ObservableProperty]
    private string _replaceExtension = "";

    [ObservableProperty]
    private bool _removeExtensions;

    [ObservableProperty]
    private string _outputDir = "";

    [ObservableProperty]
    private bool _inplace;

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

    [ObservableProperty]
    private bool _useHeaderSkipper;

    [ObservableProperty]
    private string _headerSkipper = "";

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

    #region Cleaning options

    [ObservableProperty]
    private bool _keepEmptyGames;

    [ObservableProperty]
    private bool _clean;

    [ObservableProperty]
    private bool _removeUnicode;

    [ObservableProperty]
    private bool _descriptionAsName;

    public IReadOnlyList<ComboOption> MergeModes => OptionCatalog.MergeModes;

    [ObservableProperty]
    private ComboOption _mergeMode;

    [ObservableProperty]
    private bool _trim;

    [ObservableProperty]
    private string _trimRootDir = "";

    [ObservableProperty]
    private bool _singleSet;

    #endregion

    #region Combining and diffing modes

    public IReadOnlyList<ComboOption> DedupModes => OptionCatalog.DedupModes;

    [ObservableProperty]
    private ComboOption _dedupMode;

    [ObservableProperty]
    private bool _modeMerge;

    [ObservableProperty]
    private bool _diffAll;

    [ObservableProperty]
    private bool _diffDuplicates;

    [ObservableProperty]
    private bool _diffIndividuals;

    [ObservableProperty]
    private bool _diffNoDuplicates;

    public IReadOnlyList<ComboOption> CascadeModes => OptionCatalog.CascadeModes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCascadeSelected))]
    private ComboOption _cascadeMode;

    public bool IsCascadeSelected => CascadeMode.Value.Length > 0;

    [ObservableProperty]
    private bool _skipFirstOutput;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsBaseDats))]
    private bool _diffAgainst;

    [ObservableProperty]
    private bool _byGame;

    public IReadOnlyList<ComboOption> BaseReplaceModes => OptionCatalog.BaseReplaceModes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsBaseDats))]
    [NotifyPropertyChangedFor(nameof(IsBaseReplaceSelected))]
    private ComboOption _baseReplaceMode;

    public bool IsBaseReplaceSelected => BaseReplaceMode.Value.Length > 0;

    [ObservableProperty]
    private string _updateFields = "";

    [ObservableProperty]
    private bool _onlySame;

    [ObservableProperty]
    private bool _noAutomaticDate;

    /// <summary>Base DATs used by diff-against and base-replace modes</summary>
    public ObservableCollection<string> BaseDats { get; } = [];

    /// <summary>True when a selected mode requires base DATs</summary>
    public bool NeedsBaseDats => DiffAgainst || IsBaseReplaceSelected;

    private bool AnyCombiningMode =>
        ModeMerge || DiffAll || DiffDuplicates || DiffIndividuals || DiffNoDuplicates || IsCascadeSelected;

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

    /// <summary>Modes that consume base DATs cannot run without them</summary>
    public override bool CanRun => Inputs.Count > 0 && (!NeedsBaseDats || BaseDats.Count > 0);

    protected override IEnumerable<string> InputPaths => Inputs;

    protected override IEnumerable<string> BuildOptionArguments()
    {
        // Output formats and format-dependent naming options
        foreach (FormatOption format in OutputFormats)
        {
            if (format.IsSelected)
                yield return $"--output-type={format.Value}";
        }

        if (HasFormatSelected)
        {
            if (!string.IsNullOrEmpty(Prefix))
                yield return $"--prefix={Prefix}";
            if (!string.IsNullOrEmpty(Postfix))
                yield return $"--postfix={Postfix}";
            if (Quotes)
                yield return "--quotes";
            if (RomsInMiss)
                yield return "--roms";
            if (GamePrefix)
                yield return "--game-prefix";
            if (!string.IsNullOrWhiteSpace(AddExtension))
                yield return $"--add-extension={AddExtension.Trim()}";
            if (!string.IsNullOrWhiteSpace(ReplaceExtension))
                yield return $"--replace-extension={ReplaceExtension.Trim()}";
            if (RemoveExtensions)
                yield return "--remove-extensions";

            if (Romba)
            {
                yield return "--romba";
                if (int.TryParse(RombaDepth, out int depth) && depth >= 0)
                    yield return $"--romba-depth={depth}";
            }

            if (Deprecated && IsXmlSelected)
                yield return "--deprecated";
        }

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
        if (UseHeaderSkipper)
            yield return $"--header={HeaderSkipper.Trim()}";

        if (ForceMerging.Value.Length > 0)
            yield return $"--forcemerging={ForceMerging.Value}";
        if (ForceNodump.Value.Length > 0)
            yield return $"--forcenodump={ForceNodump.Value}";
        if (ForcePacking.Value.Length > 0)
            yield return $"--forcepacking={ForcePacking.Value}";

        // Cleaning
        if (KeepEmptyGames)
            yield return "--keep-empty-games";
        if (Clean)
            yield return "--clean";
        if (RemoveUnicode)
            yield return "--remove-unicode";
        if (DescriptionAsName)
            yield return "--description-as-name";

        if (MergeMode.Value.Length > 0)
            yield return MergeMode.Value;

        if (Trim)
        {
            yield return "--trim";
            if (!string.IsNullOrWhiteSpace(TrimRootDir))
                yield return $"--root-dir={TrimRootDir.Trim()}";
        }

        if (SingleSet)
            yield return "--single-set";

        if (DedupMode.Value.Length > 0)
            yield return DedupMode.Value;

        // Combining and diffing modes
        if (ModeMerge)
            yield return "--merge";
        if (DiffAll)
            yield return "--diff-all";
        if (DiffDuplicates)
            yield return "--diff-duplicates";
        if (DiffIndividuals)
            yield return "--diff-individuals";
        if (DiffNoDuplicates)
            yield return "--diff-no-duplicates";

        if (IsCascadeSelected)
        {
            yield return CascadeMode.Value;
            if (SkipFirstOutput)
                yield return "--skip-first-output";
        }

        if (DiffAgainst)
        {
            yield return "--diff-against";
            if (ByGame)
                yield return "--by-game";
        }

        if (IsBaseReplaceSelected)
        {
            yield return BaseReplaceMode.Value;

            bool hasUpdateFields = false;
            foreach (string field in SplitMultiline(UpdateFields))
            {
                hasUpdateFields = true;
                yield return $"--update-field={field}";
            }

            if (OnlySame && hasUpdateFields)
                yield return "--only-same";
        }

        if (NeedsBaseDats)
        {
            foreach (string baseDat in BaseDats)
                yield return $"--base-dat={baseDat}";
        }

        if (NoAutomaticDate && AnyCombiningMode)
            yield return "--no-automatic-date";

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

        yield return $"--output-dir={ResolveOutputDir(OutputDir)}";
        if (Inplace)
            yield return "--inplace";
    }

    #region Presets

    private sealed class UpdatePreset
    {
        public List<string> Inputs { get; set; } = [];
        public List<string> Formats { get; set; } = [];
        public bool Deprecated { get; set; }
        public bool Romba { get; set; }
        public string RombaDepth { get; set; } = "";
        public string Prefix { get; set; } = "";
        public string Postfix { get; set; } = "";
        public bool Quotes { get; set; }
        public bool RomsInMiss { get; set; }
        public bool GamePrefix { get; set; }
        public string AddExtension { get; set; } = "";
        public string ReplaceExtension { get; set; } = "";
        public bool RemoveExtensions { get; set; }
        public string OutputDir { get; set; } = "";
        public bool Inplace { get; set; }
        public Dictionary<string, string> HeaderFields { get; set; } = [];
        public bool SuperDat { get; set; }
        public bool UseHeaderSkipper { get; set; }
        public string HeaderSkipper { get; set; } = "";
        public string ForceMerging { get; set; } = "";
        public string ForceNodump { get; set; } = "";
        public string ForcePacking { get; set; } = "";
        public bool KeepEmptyGames { get; set; }
        public bool Clean { get; set; }
        public bool RemoveUnicode { get; set; }
        public bool DescriptionAsName { get; set; }
        public string MergeMode { get; set; } = "";
        public bool Trim { get; set; }
        public string TrimRootDir { get; set; } = "";
        public bool SingleSet { get; set; }
        public string DedupMode { get; set; } = "";
        public bool ModeMerge { get; set; }
        public bool DiffAll { get; set; }
        public bool DiffDuplicates { get; set; }
        public bool DiffIndividuals { get; set; }
        public bool DiffNoDuplicates { get; set; }
        public string CascadeMode { get; set; } = "";
        public bool SkipFirstOutput { get; set; }
        public bool DiffAgainst { get; set; }
        public bool ByGame { get; set; }
        public string BaseReplaceMode { get; set; } = "";
        public string UpdateFields { get; set; } = "";
        public bool OnlySame { get; set; }
        public bool NoAutomaticDate { get; set; }
        public List<string> BaseDats { get; set; } = [];
        public bool OneGamePerRegion { get; set; }
        public string Regions { get; set; } = "";
        public bool OneRomPerGame { get; set; }
        public bool SceneDateStrip { get; set; }
        public string ExcludeFields { get; set; } = "";
        public string Filters { get; set; } = "";
        public bool MatchOfTags { get; set; }
        public string ExtraInis { get; set; } = "";
    }

    protected override object CapturePreset() => new UpdatePreset
    {
        Inputs = [.. Inputs],
        Formats = [.. OutputFormats.Where(f => f.IsSelected).Select(f => f.Value)],
        Deprecated = Deprecated,
        Romba = Romba,
        RombaDepth = RombaDepth,
        Prefix = Prefix,
        Postfix = Postfix,
        Quotes = Quotes,
        RomsInMiss = RomsInMiss,
        GamePrefix = GamePrefix,
        AddExtension = AddExtension,
        ReplaceExtension = ReplaceExtension,
        RemoveExtensions = RemoveExtensions,
        OutputDir = OutputDir,
        Inplace = Inplace,
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
        UseHeaderSkipper = UseHeaderSkipper,
        HeaderSkipper = HeaderSkipper,
        ForceMerging = ForceMerging.Value,
        ForceNodump = ForceNodump.Value,
        ForcePacking = ForcePacking.Value,
        KeepEmptyGames = KeepEmptyGames,
        Clean = Clean,
        RemoveUnicode = RemoveUnicode,
        DescriptionAsName = DescriptionAsName,
        MergeMode = MergeMode.Value,
        Trim = Trim,
        TrimRootDir = TrimRootDir,
        SingleSet = SingleSet,
        DedupMode = DedupMode.Value,
        ModeMerge = ModeMerge,
        DiffAll = DiffAll,
        DiffDuplicates = DiffDuplicates,
        DiffIndividuals = DiffIndividuals,
        DiffNoDuplicates = DiffNoDuplicates,
        CascadeMode = CascadeMode.Value,
        SkipFirstOutput = SkipFirstOutput,
        DiffAgainst = DiffAgainst,
        ByGame = ByGame,
        BaseReplaceMode = BaseReplaceMode.Value,
        UpdateFields = UpdateFields,
        OnlySame = OnlySame,
        NoAutomaticDate = NoAutomaticDate,
        BaseDats = [.. BaseDats],
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
        UpdatePreset? preset;
        try
        {
            preset = data.Deserialize<UpdatePreset>();
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

        BaseDats.Clear();
        foreach (string baseDat in preset.BaseDats.Where(p => !string.IsNullOrWhiteSpace(p)))
            BaseDats.Add(baseDat);

        foreach (FormatOption format in OutputFormats)
            format.IsSelected = preset.Formats.Contains(format.Value);

        Deprecated = preset.Deprecated;
        Romba = preset.Romba;
        RombaDepth = preset.RombaDepth;
        Prefix = preset.Prefix;
        Postfix = preset.Postfix;
        Quotes = preset.Quotes;
        RomsInMiss = preset.RomsInMiss;
        GamePrefix = preset.GamePrefix;
        AddExtension = preset.AddExtension;
        ReplaceExtension = preset.ReplaceExtension;
        RemoveExtensions = preset.RemoveExtensions;
        OutputDir = preset.OutputDir;
        Inplace = preset.Inplace;

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
        UseHeaderSkipper = preset.UseHeaderSkipper;
        HeaderSkipper = preset.HeaderSkipper;
        ForceMerging = ForceMergingModes.FirstOrDefault(m => m.Value == preset.ForceMerging) ?? ForceMergingModes[0];
        ForceNodump = ForceNodumpModes.FirstOrDefault(m => m.Value == preset.ForceNodump) ?? ForceNodumpModes[0];
        ForcePacking = ForcePackingModes.FirstOrDefault(m => m.Value == preset.ForcePacking) ?? ForcePackingModes[0];

        KeepEmptyGames = preset.KeepEmptyGames;
        Clean = preset.Clean;
        RemoveUnicode = preset.RemoveUnicode;
        DescriptionAsName = preset.DescriptionAsName;
        MergeMode = MergeModes.FirstOrDefault(m => m.Value == preset.MergeMode) ?? MergeModes[0];
        Trim = preset.Trim;
        TrimRootDir = preset.TrimRootDir;
        SingleSet = preset.SingleSet;
        DedupMode = DedupModes.FirstOrDefault(m => m.Value == preset.DedupMode) ?? DedupModes[0];

        ModeMerge = preset.ModeMerge;
        DiffAll = preset.DiffAll;
        DiffDuplicates = preset.DiffDuplicates;
        DiffIndividuals = preset.DiffIndividuals;
        DiffNoDuplicates = preset.DiffNoDuplicates;
        CascadeMode = CascadeModes.FirstOrDefault(m => m.Value == preset.CascadeMode) ?? CascadeModes[0];
        SkipFirstOutput = preset.SkipFirstOutput;
        DiffAgainst = preset.DiffAgainst;
        ByGame = preset.ByGame;
        BaseReplaceMode = BaseReplaceModes.FirstOrDefault(m => m.Value == preset.BaseReplaceMode) ?? BaseReplaceModes[0];
        UpdateFields = preset.UpdateFields;
        OnlySame = preset.OnlySame;
        NoAutomaticDate = preset.NoAutomaticDate;

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
