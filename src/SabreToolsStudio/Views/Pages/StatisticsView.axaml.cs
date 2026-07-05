using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SabreToolsStudio.ViewModels;

namespace SabreToolsStudio.Views.Pages;

public partial class StatisticsView : UserControl
{
    public StatisticsView()
    {
        InitializeComponent();
        InputsCard.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        InputsCard.AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private StatisticsViewModel? ViewModel => DataContext as StatisticsViewModel;

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (ViewModel is null)
            return;

        var files = e.Data.GetFiles();
        if (files is not null)
        {
            ViewModel.AddInputs(files
                .Select(f => f.TryGetLocalPath())
                .Where(p => !string.IsNullOrEmpty(p))!);
        }
    }

    private async void OnAddFilesClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null || TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select DAT files",
            AllowMultiple = true,
        });

        ViewModel.AddInputs(files
            .Select(f => f.TryGetLocalPath())
            .Where(p => !string.IsNullOrEmpty(p))!);
    }

    private async void OnAddFolderClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null || TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select a folder of DAT files",
            AllowMultiple = true,
        });

        ViewModel.AddInputs(folders
            .Select(f => f.TryGetLocalPath())
            .Where(p => !string.IsNullOrEmpty(p))!);
    }

    private async void OnBrowseOutputClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null || TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select the output directory",
            AllowMultiple = false,
        });

        if (folders.Count > 0 && folders[0].TryGetLocalPath() is string path)
            ViewModel.OutputDir = path;
    }
}
