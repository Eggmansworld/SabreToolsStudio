using System.ComponentModel;
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
}
