using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SabreToolsStudio.ViewModels;

namespace SabreToolsStudio.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnCopyCommandClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && Clipboard is not null && vm.CommandPreview.Length > 0)
            await Clipboard.SetTextAsync(vm.CommandPreview);
    }

    private void OnOpenWikiClick(object? sender, RoutedEventArgs e) =>
        OpenUrl("https://github.com/SabreTools/SabreTools/wiki");

    private void OnOpenRepoClick(object? sender, RoutedEventArgs e) =>
        OpenUrl("https://github.com/SabreTools/SabreTools");

    private static void OpenUrl(string url)
    {
        try
        {
            // UseShellExecute routes to the default browser on Windows/macOS/Linux
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Opening a browser is best-effort only
        }
    }
}
