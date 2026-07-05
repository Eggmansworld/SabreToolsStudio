using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SabreToolsStudio.ViewModels;

namespace SabreToolsStudio.Views.Pages;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private async void OnBrowseExeClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm || TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Locate the SabreTools executable",
            AllowMultiple = false,
        });

        if (files.Count > 0 && files[0].TryGetLocalPath() is string path)
            vm.SetExecutablePath(path);
    }
}
