using Lorcaire.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Composition;

public static class DesktopDependencyInjection
{
    public static IServiceCollection AddLorcaireDesktop(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPageFactory, PageFactory>();
        services.AddSingleton<MainWindow>();

        services.AddTransient<DashboardView>();
        services.AddTransient<GoalsView>();
        services.AddTransient<ProjectsView>();
        services.AddTransient<TasksView>();
        services.AddTransient<ResourcesView>();
        services.AddTransient<CalendarView>();
        services.AddTransient<NotesView>();
        services.AddTransient<SettingsView>();

        return services;
    }
}
