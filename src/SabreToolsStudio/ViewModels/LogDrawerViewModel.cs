using System.Diagnostics;
using System.Text;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SabreToolsStudio.Services;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// State for the sliding log drawer: buffers process output off the UI thread
/// and flushes it on a timer, tracks run status and elapsed time.
/// </summary>
public partial class LogDrawerViewModel : ViewModelBase
{
    private static readonly IBrush _idleBrush = new SolidColorBrush(Color.Parse("#8A8F98"));
    private static readonly IBrush _runningBrush = new SolidColorBrush(Color.Parse("#E8A33D"));
    private static readonly IBrush _successBrush = new SolidColorBrush(Color.Parse("#3FB950"));
    private static readonly IBrush _errorBrush = new SolidColorBrush(Color.Parse("#E5484D"));

    private readonly StringBuilder _buffer = new();
    private readonly object _bufferLock = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly DispatcherTimer _timer;
    private bool _dirty;

    public LogDrawerViewModel(CliRunner runner)
    {
        runner.OutputLine += OnOutputLine;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _timer.Tick += (_, _) =>
        {
            FlushBuffer();
            if (_stopwatch.IsRunning)
                Elapsed = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        };
        _timer.Start();
    }

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "Idle";

    [ObservableProperty]
    private string _lastLine = "";

    [ObservableProperty]
    private string _elapsed = "";

    [ObservableProperty]
    private string _logText = "";

    [ObservableProperty]
    private IBrush _statusBrush = _idleBrush;

    [RelayCommand]
    private void ToggleExpanded() => IsExpanded = !IsExpanded;

    [RelayCommand]
    private void Clear()
    {
        lock (_bufferLock)
        {
            _buffer.Clear();
            _dirty = false;
        }
        LogText = "";
        LastLine = "";
    }

    /// <summary>Called on the UI thread when a run begins</summary>
    public void OnRunStarted(string commandPreview)
    {
        lock (_bufferLock)
        {
            if (_buffer.Length > 0)
                _buffer.AppendLine();
            _buffer.Append("> ").AppendLine(commandPreview);
            _dirty = true;
        }

        IsRunning = true;
        IsExpanded = true;
        StatusText = "Running";
        StatusBrush = _runningBrush;
        _stopwatch.Restart();
        Elapsed = "00:00:00";
        FlushBuffer();
    }

    /// <summary>Called on the UI thread when a run finishes</summary>
    public void OnRunCompleted(int exitCode, bool cancelled)
    {
        _stopwatch.Stop();
        Elapsed = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        IsRunning = false;

        (StatusText, StatusBrush) = (cancelled, exitCode) switch
        {
            (true, _) => ("Cancelled", _idleBrush),
            (false, 0) => ("Completed", _successBrush),
            _ => ($"Failed (exit code {exitCode})", _errorBrush),
        };

        AppendLine($"--- {StatusText} in {Elapsed} ---");
        FlushBuffer();
    }

    /// <summary>Report a GUI-side problem (e.g. missing executable) into the log</summary>
    public void ReportError(string message)
    {
        AppendLine("ERROR: " + message);
        StatusText = "Error";
        StatusBrush = _errorBrush;
        IsExpanded = true;
        FlushBuffer();
    }

    private void AppendLine(string line)
    {
        lock (_bufferLock)
        {
            _buffer.AppendLine(line);
            _dirty = true;
        }
    }

    // Called from background threads by CliRunner
    private void OnOutputLine(string line) => AppendLine(line);

    private void FlushBuffer()
    {
        string? text = null;
        lock (_bufferLock)
        {
            if (_dirty)
            {
                text = _buffer.ToString();
                _dirty = false;
            }
        }

        if (text is null)
            return;

        LogText = text;

        int end = text.Length;
        while (end > 0 && (text[end - 1] == '\r' || text[end - 1] == '\n'))
            end--;
        int start = end;
        while (start > 0 && text[start - 1] != '\n')
            start--;
        LastLine = text[start..end];
    }
}
