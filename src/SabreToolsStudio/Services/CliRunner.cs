using System.Diagnostics;

namespace SabreToolsStudio.Services;

/// <summary>
/// Runs the SabreTools CLI as a child process, streaming stdout/stderr line by line.
/// Output events are raised on background threads; consumers must marshal to the UI thread.
/// </summary>
public sealed class CliRunner
{
    private Process? _process;
    private bool _cancelRequested;

    public bool IsRunning { get; private set; }

    /// <summary>Raised for each line of process output (stdout and stderr). Background thread!</summary>
    public event Action<string>? OutputLine;

    /// <summary>
    /// Launches the executable with the given arguments and waits for exit.
    /// Returns (exitCode, cancelled).
    /// </summary>
    public async Task<(int ExitCode, bool Cancelled)> RunAsync(string exePath, IReadOnlyList<string> arguments)
    {
        if (IsRunning)
            throw new InvalidOperationException("An operation is already running.");

        var psi = new ProcessStartInfo(exePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(exePath) ?? AppContext.BaseDirectory,
        };
        foreach (string arg in arguments)
            psi.ArgumentList.Add(arg);

        _cancelRequested = false;
        _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        _process.OutputDataReceived += (_, e) => { if (e.Data is not null) OutputLine?.Invoke(e.Data); };
        _process.ErrorDataReceived += (_, e) => { if (e.Data is not null) OutputLine?.Invoke(e.Data); };

        IsRunning = true;
        try
        {
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            await _process.WaitForExitAsync();
            return (_process.ExitCode, _cancelRequested);
        }
        finally
        {
            IsRunning = false;
            _process.Dispose();
            _process = null;
        }
    }

    /// <summary>Kills the running process tree, if any</summary>
    public void Cancel()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _cancelRequested = true;
                _process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Process may have exited between the check and the kill
        }
    }
}
