using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace SabreToolsStudio.Views.Controls;

/// <summary>
/// Reusable file/folder path list with picker buttons and drag-and-drop
/// </summary>
public partial class PathListEditor : UserControl
{
    public static readonly StyledProperty<ObservableCollection<string>?> ItemsSourceProperty =
        AvaloniaProperty.Register<PathListEditor, ObservableCollection<string>?>(nameof(ItemsSource));

    public static readonly StyledProperty<bool> AllowFilesProperty =
        AvaloniaProperty.Register<PathListEditor, bool>(nameof(AllowFiles), defaultValue: true);

    public static readonly StyledProperty<bool> AllowFoldersProperty =
        AvaloniaProperty.Register<PathListEditor, bool>(nameof(AllowFolders), defaultValue: true);

    public static readonly StyledProperty<string> EmptyTextProperty =
        AvaloniaProperty.Register<PathListEditor, string>(nameof(EmptyText), defaultValue: "No paths added yet.");

    public static readonly StyledProperty<string> PickerTitleProperty =
        AvaloniaProperty.Register<PathListEditor, string>(nameof(PickerTitle), defaultValue: "Select files");

    private ObservableCollection<string>? _items;

    public PathListEditor()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        UpdateVisibility();
    }

    public ObservableCollection<string>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public bool AllowFiles
    {
        get => GetValue(AllowFilesProperty);
        set => SetValue(AllowFilesProperty, value);
    }

    public bool AllowFolders
    {
        get => GetValue(AllowFoldersProperty);
        set => SetValue(AllowFoldersProperty, value);
    }

    public string EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public string PickerTitle
    {
        get => GetValue(PickerTitleProperty);
        set => SetValue(PickerTitleProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            if (_items is not null)
                _items.CollectionChanged -= OnItemsChanged;

            _items = ItemsSource;
            PathsList.ItemsSource = _items;
            if (_items is not null)
                _items.CollectionChanged += OnItemsChanged;

            UpdateVisibility();
        }
        else if (change.Property == AllowFilesProperty
            || change.Property == AllowFoldersProperty
            || change.Property == EmptyTextProperty)
        {
            UpdateVisibility();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateVisibility();

    private void UpdateVisibility()
    {
        AddFilesButton.IsVisible = AllowFiles;
        AddFolderButton.IsVisible = AllowFolders;
        EmptyHint.Text = EmptyText;
        EmptyHint.IsVisible = _items is null || _items.Count == 0;
    }

    private void AddPaths(IEnumerable<string?> paths)
    {
        if (_items is null)
            return;

        foreach (string? path in paths)
        {
            if (!string.IsNullOrWhiteSpace(path) && !_items.Contains(path))
                _items.Add(path);
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();
        if (files is not null)
            AddPaths(files.Select(f => f.TryGetLocalPath()));
    }

    private async void OnAddFilesClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = PickerTitle,
            AllowMultiple = true,
        });
        AddPaths(files.Select(f => f.TryGetLocalPath()));
    }

    private async void OnAddFolderClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = PickerTitle,
            AllowMultiple = true,
        });
        AddPaths(folders.Select(f => f.TryGetLocalPath()));
    }

    private void OnRemoveClick(object? sender, RoutedEventArgs e)
    {
        if (_items is not null && PathsList.SelectedItem is string selected)
            _items.Remove(selected);
    }

    private void OnClearClick(object? sender, RoutedEventArgs e) => _items?.Clear();
}
