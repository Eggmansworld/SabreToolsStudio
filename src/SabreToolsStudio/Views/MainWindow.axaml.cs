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
}
