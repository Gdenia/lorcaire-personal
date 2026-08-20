using Avalonia.Controls;
using Avalonia.Media;
using Lorcaire.Application.Settings;
using Lorcaire.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Composition;

public sealed class PageFactory : IPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PageFactory(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    public Control Create(
        DesktopPage page,
        Action<DesktopPage> navigate,
        Action<string> greetingChanged,
        Action<UserPreferences> preferencesSaved)
    {
        ArgumentNullException.ThrowIfNull(navigate);
        ArgumentNullException.ThrowIfNull(greetingChanged);
        ArgumentNullException.ThrowIfNull(preferencesSaved);

        return page switch
        {
            DesktopPage.Dashboard => CreateDashboard(
                navigate,
                greetingChanged),
            DesktopPage.Goals =>
                _serviceProvider.GetRequiredService<GoalsView>(),
            DesktopPage.Projects =>
                _serviceProvider.GetRequiredService<ProjectsView>(),
            DesktopPage.Tasks =>
                _serviceProvider.GetRequiredService<TasksView>(),
            DesktopPage.Resources =>
                _serviceProvider.GetRequiredService<ResourcesView>(),
            DesktopPage.Calendar =>
                _serviceProvider.GetRequiredService<CalendarView>(),
            DesktopPage.Notes =>
                _serviceProvider.GetRequiredService<NotesView>(),
            DesktopPage.Settings =>
                ActivatorUtilities.CreateInstance<SettingsView>(
                    _serviceProvider,
                    preferencesSaved),
            DesktopPage.About => CreateAbout(),
            _ => throw new ArgumentOutOfRangeException(nameof(page), page, null)
        };
    }

    private DashboardView CreateDashboard(
        Action<DesktopPage> navigate,
        Action<string> greetingChanged)
    {
        Action<DashboardDestination> navigateFromDashboard =
            destination => navigate(destination switch
            {
                DashboardDestination.Goals => DesktopPage.Goals,
                DashboardDestination.Projects => DesktopPage.Projects,
                DashboardDestination.Tasks => DesktopPage.Tasks,
                DashboardDestination.Calendar => DesktopPage.Calendar,
                DashboardDestination.Notes => DesktopPage.Notes,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(destination),
                    destination,
                    null)
            });

        return ActivatorUtilities.CreateInstance<DashboardView>(
            _serviceProvider,
            navigateFromDashboard,
            greetingChanged);
    }

    private static TextBlock CreateAbout() =>
        new()
        {
            Text = "About",
            FontSize = 26,
            Foreground = Brushes.White
        };
}
