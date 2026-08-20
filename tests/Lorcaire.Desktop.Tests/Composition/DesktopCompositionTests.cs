using Lorcaire.Bootstrap;
using Lorcaire.Desktop;
using Lorcaire.Desktop.Composition;
using Lorcaire.Desktop.Views;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Tests.Composition;

public sealed class DesktopCompositionTests
{
    [Fact]
    public void CreateServiceProvider_ValidatesCompleteDesktopGraph()
    {
        using var dataDirectory = TemporaryDataDirectory.Create();
        using var provider = (ServiceProvider)
            DependencyInjection.CreateServiceProvider(
                services => services.AddLorcaireDesktop(),
                dataDirectory.Path);

        Assert.NotNull(provider.GetRequiredService<IPageFactory>());

        var registeredServices =
            provider.GetRequiredService<IServiceProviderIsService>();

        Type[] expectedDesktopTypes =
        [
            typeof(MainWindow),
            typeof(DashboardView),
            typeof(GoalsView),
            typeof(ProjectsView),
            typeof(TasksView),
            typeof(ResourcesView),
            typeof(CalendarView),
            typeof(NotesView),
            typeof(SettingsView)
        ];

        Assert.All(
            expectedDesktopTypes,
            type => Assert.True(
                registeredServices.IsService(type),
                $"Desktop composition does not register '{type.Name}'."));
    }

    [Fact]
    public void CreateServiceProvider_ResolvesAllRegisteredViews()
    {
        using var dataDirectory = TemporaryDataDirectory.Create();
        using var provider = (ServiceProvider)
            DependencyInjection.CreateServiceProvider(
                services => services.AddLorcaireDesktop(),
                dataDirectory.Path);

        Assert.NotNull(provider.GetRequiredService<DashboardView>());
        Assert.NotNull(provider.GetRequiredService<GoalsView>());
        Assert.NotNull(provider.GetRequiredService<ProjectsView>());
        Assert.NotNull(provider.GetRequiredService<TasksView>());
        Assert.NotNull(provider.GetRequiredService<ResourcesView>());
        Assert.NotNull(provider.GetRequiredService<CalendarView>());
        Assert.NotNull(provider.GetRequiredService<NotesView>());
        Assert.NotNull(provider.GetRequiredService<SettingsView>());
    }

    [Fact]
    public void PageFactory_CreatesEveryPageAndFreshInstances()
    {
        using var dataDirectory = TemporaryDataDirectory.Create();
        using var provider = (ServiceProvider)
            DependencyInjection.CreateServiceProvider(
                services => services.AddLorcaireDesktop(),
                dataDirectory.Path);
        var factory = provider.GetRequiredService<IPageFactory>();

        foreach (var page in Enum.GetValues<DesktopPage>())
        {
            Assert.NotNull(factory.Create(
                page,
                _ => { },
                _ => { },
                _ => { }));
        }

        var firstDashboard = factory.Create(
            DesktopPage.Dashboard,
            _ => { },
            _ => { },
            _ => { });
        var secondDashboard = factory.Create(
            DesktopPage.Dashboard,
            _ => { },
            _ => { },
            _ => { });

        Assert.NotSame(firstDashboard, secondDashboard);
    }

    private sealed class TemporaryDataDirectory : IDisposable
    {
        private TemporaryDataDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryDataDirectory Create() =>
            new(System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Lorcaire.Tests",
                Guid.NewGuid().ToString("N")));

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
