using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SabreToolsStudio.ViewModels;

namespace SabreToolsStudio.Views.Controls;

public partial class LogDrawer : UserControl
{
    private const double CollapsedHeight = 46;
    private const double ExpandedHeight = 300;

    private LogDrawerViewModel? _viewModel;

    public LogDrawer()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = DataContext as LogDrawerViewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateHeight();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(LogDrawerViewModel.IsExpanded):
                UpdateHeight();
                break;
            case nameof(LogDrawerViewModel.LogText):
                // Keep the newest output in view
                LogBox.CaretIndex = LogBox.Text?.Length ?? 0;
                break;
        }
    }

    private void UpdateHeight() =>
        DrawerRoot.Height = _viewModel?.IsExpanded == true ? ExpandedHeight : CollapsedHeight;

    private async void OnCopyLogClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (_viewModel is not null && clipboard is not null && _viewModel.LogText.Length > 0)
            await clipboard.SetTextAsync(_viewModel.LogText);
    }

    #region Log context menu

    /// <summary>Extract a usable path from the highlighted log text</summary>
    private string? GetSelectedPath()
    {
        string? selection = LogBox.SelectedText?.Trim().Trim('\'', '"');
        if (string.IsNullOrEmpty(selection))
            return null;

        // Log lines often end with punctuation after the quoted path
        selection = selection.TrimEnd('.', ',', ';');
        return File.Exists(selection) || Directory.Exists(selection) ? selection : null;
    }

    private void OnLogMenuOpening(object? sender, CancelEventArgs e)
    {
        OpenInExplorerItem.IsEnabled = GetSelectedPath() is not null;
    }

    private void OnCopySelectionClick(object? sender, RoutedEventArgs e) => LogBox.Copy();

    private void OnSelectAllClick(object? sender, RoutedEventArgs e) => LogBox.SelectAll();

    private void OnOpenInExplorerClick(object? sender, RoutedEventArgs e)
    {
        if (GetSelectedPath() is not string path)
            return;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                // Select files in their folder; open folders directly
                if (File.Exists(path))
                    Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = false });
                else
                    Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = false });
            }
            else if (OperatingSystem.IsMacOS())
            {
                var psi = new ProcessStartInfo("open");
                if (File.Exists(path))
                    psi.ArgumentList.Add("-R");
                psi.ArgumentList.Add(path);
                Process.Start(psi);
            }
            else
            {
                // Linux: open the containing folder for files
                string target = File.Exists(path) ? Path.GetDirectoryName(path) ?? path : path;
                var psi = new ProcessStartInfo("xdg-open");
                psi.ArgumentList.Add(target);
                Process.Start(psi);
            }
        }
        catch
        {
            // Opening a file manager is best-effort only
        }
    }

    #endregion
}
