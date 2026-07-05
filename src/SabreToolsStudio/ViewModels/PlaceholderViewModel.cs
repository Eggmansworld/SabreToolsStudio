using Avalonia.Media;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// Stand-in page for features scheduled in later build sessions
/// </summary>
public class PlaceholderViewModel(string title, string description, int plannedSession, StreamGeometry icon) : ViewModelBase
{
    public string Title { get; } = title;
    public string Description { get; } = description;
    public string PlannedText { get; } = $"This feature is planned for build session {plannedSession}.";
    public StreamGeometry Icon { get; } = icon;
}
