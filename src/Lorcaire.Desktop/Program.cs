using Avalonia;
using Lorcaire.Bootstrap;

namespace Lorcaire.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var serviceProvider =
            DependencyInjection.CreateServiceProvider();

        App.Services = serviceProvider;

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
    }
}
