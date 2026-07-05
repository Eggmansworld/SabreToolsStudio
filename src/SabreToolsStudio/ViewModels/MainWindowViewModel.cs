using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SettingsService _settings;
    private readonly PresetService _presetService = new();
    private readonly SabreToolsLocator _locator;
    private readonly CliRunner _runner = new();

    public MainWindowViewModel(SettingsService settings)
    {
        _settings = settings;
        _locator = new SabreToolsLocator(settings);
        LogDrawer = new LogDrawerViewModel(_runner);

        var statistics = new StatisticsViewModel(settings, _presetService);
        var datFromDir = new DatFromDirViewModel(settings, _presetService);
        var verify = new VerifyViewModel(settings, _presetService);
        var sort = new SortViewModel(settings, _presetService);
        var split = new SplitViewModel(settings, _presetService);
        var update = new UpdateViewModel(settings, _presetService);

        PrimaryNav =
        [
            new("dfd", "DAT From Dir", Icons.FolderPlus, datFromDir, Navigate),
            new("sort", "Sort / Rebuild", Icons.SwapHorizontal, sort, Navigate),
            new("verify", "Verify", Icons.CheckCircle, verify, Navigate),
            new("update", "Update DATs", Icons.Update, update, Navigate),
            new("split", "Split DATs", Icons.CallSplit, split, Navigate),
            new("stats", "Statistics", Icons.ChartBar, statistics, Navigate),
            new("batch", "Batch", Icons.PlaylistPlay,
                new PlaceholderViewModel("Batch Running",
                    "Run scripted sequences of DAT operations from batch files, with a visual step builder.",
                    5, Icons.PlaylistPlay), Navigate),
        ];

        AdvancedNav =
        [
            new("headers", "Headers", Icons.FileDocument,
                new PlaceholderViewModel("Extract and Restore Headers",
                    "Detect, remove, store, and re-apply copier headers (NES, FDS, SNES, and more) on files.",
                    5, Icons.FileDocument), Navigate),
            new("settings", "Settings", Icons.Cog, new SettingsViewModel(settings, _locator), Navigate),
        ];

        Navigate(PrimaryNav.First(item => item.Key == "dfd"));
    }

    public ObservableCollection<NavItemViewModel> PrimaryNav { get; }
    public ObservableCollection<NavItemViewModel> AdvancedNav { get; }
    public LogDrawerViewModel LogDrawer { get; }

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private string _commandPreview = "";

    [ObservableProperty]
    private bool _hasCommand;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelRunCommand))]
    private bool _isRunning;

    private void Navigate(NavItemViewModel target)
    {
        foreach (NavItemViewModel item in PrimaryNav)
            item.IsSelected = item == target;
        foreach (NavItemViewModel item in AdvancedNav)
            item.IsSelected = item == target;

        CurrentPage = target.Page;
    }

    partial void OnCurrentPageChanged(ViewModelBase? oldValue, ViewModelBase? newValue)
    {
        if (oldValue is FeaturePageViewModel oldFeature)
            oldFeature.PropertyChanged -= OnPagePropertyChanged;
        if (newValue is FeaturePageViewModel newFeature)
            newFeature.PropertyChanged += OnPagePropertyChanged;

        UpdatePreview();
    }

    private void OnPagePropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdatePreview();

    private void UpdatePreview()
    {
        if (CurrentPage is FeaturePageViewModel feature)
        {
            CommandPreview = feature.CommandPreview;
            HasCommand = true;
        }
        else
        {
            CommandPreview = "";
            HasCommand = false;
        }

        RunCommand.NotifyCanExecuteChanged();
    }

    private bool CanRun() =>
        !IsRunning && CurrentPage is FeaturePageViewModel { CanRun: true };

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAsync()
    {
        if (CurrentPage is not FeaturePageViewModel feature)
            return;

        string? exePath = _locator.Resolve();
        if (exePath is null)
        {
            LogDrawer.ReportError("SabreTools executable not found. Set the path on the Settings page.");
            return;
        }

        LogDrawer.OnRunStarted(feature.CommandPreview);
        IsRunning = true;
        try
        {
            (int exitCode, bool cancelled) = await _runner.RunAsync(exePath, feature.BuildFullArguments());
            LogDrawer.OnRunCompleted(exitCode, cancelled);
        }
        catch (Exception ex)
        {
            LogDrawer.ReportError($"Failed to run SabreTools: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void CancelRun() => _runner.Cancel();
}
