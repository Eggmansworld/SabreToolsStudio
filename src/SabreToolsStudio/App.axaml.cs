using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SabreToolsStudio.Services;
using SabreToolsStudio.ViewModels;
using SabreToolsStudio.Views;

namespace SabreToolsStudio;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settings = new SettingsService();
            settings.Load();
            SettingsViewModel.ApplyTheme(settings.Current.Theme);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(settings),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
