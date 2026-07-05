using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SabreToolsStudio.ViewModels;

namespace SabreToolsStudio.Views.Pages;

public partial class BatchView : UserControl
{
    public BatchView()
    {
        InitializeComponent();
    }

    private async void OnSaveScriptClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not BatchViewModel vm || TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save batch script",
            SuggestedFileName = "batch.txt",
            DefaultExtension = "txt",
            FileTypeChoices = [new FilePickerFileType("Batch scripts") { Patterns = ["*.txt"] }],
        });

        if (file?.TryGetLocalPath() is string path)
        {
            await File.WriteAllTextAsync(path, vm.ScriptPreview);
            vm.OnScriptSaved(path);
        }
    }
}
