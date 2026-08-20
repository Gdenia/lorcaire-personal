using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lorcaire.Bootstrap;
using Lorcaire.Desktop.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop;

public partial class App : Avalonia.Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _serviceProvider =
                DependencyInjection.CreateServiceProvider(
                    services => services.AddLorcaireDesktop());

            desktop.MainWindow =
                _serviceProvider.GetRequiredService<MainWindow>();
            desktop.Exit += DisposeServiceProvider;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisposeServiceProvider(
        object? sender,
        ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _serviceProvider = null;
    }
}
