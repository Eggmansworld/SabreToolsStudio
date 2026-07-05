using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// Specialized DAT Splitting feature (split): split input DATs by various criteria.
/// Multiple split types may be applied in a single run.
/// </summary>
public partial class SplitViewModel : FeaturePageViewModel
{
    public SplitViewModel(SettingsService settings, PresetService presets)
        : base(settings, presets)
    {
        OutputFormats = new ObservableCollection<FormatOption>(OptionCatalog.CreateDatFormats());

        Inputs.CollectionChanged += (_, _) => NotifyCommandChanged();
        foreach (FormatOption format in OutputFormats)
            format.PropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsXmlSelected));
                NotifyCommandChanged();
            };
    }

    public override string Title => "Split DATs";
    public override string Description =>
        "Split input DATs by extension, best-available hash, name level, file size, total game size, or item type. More than one split type may be applied at a time.";
    public override string Flag => "split";
    public override string FeatureKey => "split";

    /// <summary>Input DAT files or folders of DATs</summary>
    public ObservableCollection<string> Inputs { get; } = [];

    #region Split modes

    [ObservableProperty]
    private bool _byExtension;

    [ObservableProperty]
    private string _extensionsA = "";

    [ObservableProperty]
    private string _extensionsB = "";

    [ObservableProperty]
    private bool _byHash;

    [ObservableProperty]
    private bool _byLevel;

    [ObservableProperty]
    private bool _shortNames;

    [ObservableProperty]
    private bool _baseNames;

    [ObservableProperty]
    private bool _bySize;

    [ObservableProperty]
    private string _radix = "";

    [ObservableProperty]
    private bool _byTotalSize;

    [ObservableProperty]
    private string _chunkSize = "";

    [ObservableProperty]
    private bool _byType;

    /// <summary>True when at least one split mode is selected</summary>
    public bool HasSplitMode => ByExtension || ByHash || ByLevel || BySize || ByTotalSize || ByType;

    #endregion

    #region Output options

    public ObservableCollection<FormatOption> OutputFormats { get; }

    public bool IsXmlSelected => OutputFormats.Any(f => f is { Value: "xml", IsSelected: true });

    [ObservableProperty]
    private bool _deprecated;

    [ObservableProperty]
    private string _outputDir = "";

    [ObservableProperty]
    private bool _inplace;

    #endregion

    public override bool CanRun => Inputs.Count > 0 && HasSplitMode;

    protected override IEnumerable<string> InputPaths => Inputs;

    protected override IEnumerable<string> BuildOptionArguments()
    {
        if (ByExtension)
        {
            yield return "--extension";
            foreach (string ext in SplitMultiline(ExtensionsA))
                yield return $"--exta={ext.TrimStart('.')}";
            foreach (string ext in SplitMultiline(ExtensionsB))
                yield return $"--extb={ext.TrimStart('.')}";
        }

        if (ByHash)
            yield return "--hash";

        if (ByLevel)
        {
            yield return "--level";
            if (ShortNames)
                yield return "--short";
            if (BaseNames)
                yield return "--base";
        }

        if (BySize)
        {
            yield return "--size";
            if (long.TryParse(Radix, out long radix) && radix > 0)
                yield return $"--radix={radix}";
        }

        if (ByTotalSize)
        {
            yield return "--total-size";
            if (long.TryParse(ChunkSize, out long chunkSize) && chunkSize > 0)
                yield return $"--chunk-size={chunkSize}";
        }

        if (ByType)
            yield return "--type";

        foreach (FormatOption format in OutputFormats)
        {
            if (format.IsSelected)
                yield return $"--output-type={format.Value}";
        }

        if (Deprecated && IsXmlSelected)
            yield return "--deprecated";

        if (!string.IsNullOrWhiteSpace(OutputDir))
            yield return $"--output-dir={OutputDir.Trim()}";
        if (Inplace)
            yield return "--inplace";
    }

    #region Presets

    private sealed class SplitPreset
    {
        public List<string> Inputs { get; set; } = [];
        public bool ByExtension { get; set; }
        public string ExtensionsA { get; set; } = "";
        public string ExtensionsB { get; set; } = "";
        public bool ByHash { get; set; }
        public bool ByLevel { get; set; }
        public bool ShortNames { get; set; }
        public bool BaseNames { get; set; }
        public bool BySize { get; set; }
        public string Radix { get; set; } = "";
        public bool ByTotalSize { get; set; }
        public string ChunkSize { get; set; } = "";
        public bool ByType { get; set; }
        public List<string> Formats { get; set; } = [];
        public bool Deprecated { get; set; }
        public string OutputDir { get; set; } = "";
        public bool Inplace { get; set; }
    }

    protected override object CapturePreset() => new SplitPreset
    {
        Inputs = [.. Inputs],
        ByExtension = ByExtension,
        ExtensionsA = ExtensionsA,
        ExtensionsB = ExtensionsB,
        ByHash = ByHash,
        ByLevel = ByLevel,
        ShortNames = ShortNames,
        BaseNames = BaseNames,
        BySize = BySize,
        Radix = Radix,
        ByTotalSize = ByTotalSize,
        ChunkSize = ChunkSize,
        ByType = ByType,
        Formats = [.. OutputFormats.Where(f => f.IsSelected).Select(f => f.Value)],
        Deprecated = Deprecated,
        OutputDir = OutputDir,
        Inplace = Inplace,
    };

    protected override void ApplyPreset(JsonElement data)
    {
        SplitPreset? preset;
        try
        {
            preset = data.Deserialize<SplitPreset>();
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

        ByExtension = preset.ByExtension;
        ExtensionsA = preset.ExtensionsA;
        ExtensionsB = preset.ExtensionsB;
        ByHash = preset.ByHash;
        ByLevel = preset.ByLevel;
        ShortNames = preset.ShortNames;
        BaseNames = preset.BaseNames;
        BySize = preset.BySize;
        Radix = preset.Radix;
        ByTotalSize = preset.ByTotalSize;
        ChunkSize = preset.ChunkSize;
        ByType = preset.ByType;

        foreach (FormatOption format in OutputFormats)
            format.IsSelected = preset.Formats.Contains(format.Value);

        Deprecated = preset.Deprecated;
        OutputDir = preset.OutputDir;
        Inplace = preset.Inplace;
    }

    #endregion
}
