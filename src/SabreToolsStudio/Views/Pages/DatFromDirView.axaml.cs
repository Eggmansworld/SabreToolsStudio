using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SabreToolsStudio.ViewModels;

namespace SabreToolsStudio.Views.Pages;

public partial class DatFromDirView : UserControl
{
    public DatFromDirView()
    {
        InitializeComponent();
    }

    private async void OnBrowseOutputClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not DatFromDirViewModel vm || TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select the output directory",
            AllowMultiple = false,
        });

        if (folders.Count > 0 && folders[0].TryGetLocalPath() is string path)
            vm.OutputDir = path;
    }
}
