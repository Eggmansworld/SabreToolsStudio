using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// One entry in the navigation rail
/// </summary>
public partial class NavItemViewModel(
    string key,
    string title,
    StreamGeometry icon,
    ViewModelBase page,
    Action<NavItemViewModel> navigate) : ViewModelBase
{
    public string Key { get; } = key;
    public string Title { get; } = title;
    public StreamGeometry Icon { get; } = icon;
    public ViewModelBase Page { get; } = page;

    [ObservableProperty]
    private bool _isSelected;

    [RelayCommand]
    private void Select() => navigate(this);
}
