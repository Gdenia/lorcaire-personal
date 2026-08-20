using Lorcaire.Application;
using Lorcaire.Application.Calendar.CreateCalendarEvent;
using Lorcaire.Application.Calendar.GetCalendarEvents;
using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Application.Dashboard;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.GetGoals;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Notes.CreateNote;
using Lorcaire.Application.Notes.GetNotes;
using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Application.Notes.UpdateNote;
using Lorcaire.Application.Projects.CreateProject;
using Lorcaire.Application.Projects.GetProjects;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Resources.CreateResource;
using Lorcaire.Application.Resources.GetResources;
using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Application.Settings;
using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.CreateTask;
using Lorcaire.Application.Tasks.GetTasks;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Infrastructure.Persistence.Sqlite;
using Lorcaire.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Bootstrap;

public static class DependencyInjection
{
    private static readonly Guid DefaultAreaId =
        Guid.Parse("a8324f29-1517-4bd8-a15d-cf4fdc61ad35");

    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        var workspaceContext =
            new WorkspaceContext(DefaultAreaId);

        var personalDataPath = GetPersonalDataPath();
        var databasePath = Path.Combine(
            personalDataPath,
            "lorcaire.db");
        var settingsPath = Path.Combine(
            personalDataPath,
            "settings.json");

        var connectionFactory =
            new SqliteConnectionFactory(databasePath);

        var databaseInitializer =
            new SqliteDatabaseInitializer(connectionFactory);

        databaseInitializer
            .InitializeAsync(
                new AreaId(workspaceContext.DefaultAreaId))
            .GetAwaiter()
            .GetResult();

        services.AddSingleton(workspaceContext);
        services.AddSingleton(connectionFactory);
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IUserPreferencesStore>(
            new JsonUserPreferencesStore(settingsPath));
        services.AddTransient<GetUserPreferencesHandler>();
        services.AddTransient<SaveUserPreferencesHandler>();

        services.AddSingleton<SqliteCalendarEventRepository>();

        services.AddSingleton<ICalendarEventRepository>(
            provider =>
                provider.GetRequiredService<
                    SqliteCalendarEventRepository>());

        services.AddSingleton<ICalendarEventReader>(
            provider =>
                provider.GetRequiredService<
                    SqliteCalendarEventRepository>());

        services.AddTransient<CreateCalendarEventHandler>();
        services.AddTransient<GetCalendarEventsHandler>();
        services.AddTransient<GetDashboardHandler>();

        services.AddSingleton<
            IAreaRepository,
            SqliteAreaRepository>();

        services.AddSingleton<SqliteGoalRepository>();

        services.AddSingleton<IGoalRepository>(
            provider =>
                provider.GetRequiredService<
                    SqliteGoalRepository>());

        services.AddSingleton<IGoalReader>(
            provider =>
                provider.GetRequiredService<
                    SqliteGoalRepository>());

        services.AddTransient<CreateGoalHandler>();
        services.AddTransient<GetGoalsHandler>();

        services.AddSingleton<SqliteNoteRepository>();

        services.AddSingleton<INoteRepository>(
            provider =>
                provider.GetRequiredService<
                    SqliteNoteRepository>());

        services.AddSingleton<INoteReader>(
            provider =>
                provider.GetRequiredService<
                    SqliteNoteRepository>());

        services.AddTransient<CreateNoteHandler>();
        services.AddTransient<GetNotesHandler>();
        services.AddTransient<UpdateNoteHandler>();

        services.AddSingleton<SqliteProjectRepository>();

        services.AddSingleton<IProjectRepository>(
            provider =>
                provider.GetRequiredService<
                    SqliteProjectRepository>());

        services.AddSingleton<IProjectReader>(
            provider =>
                provider.GetRequiredService<
                    SqliteProjectRepository>());

        services.AddTransient<CreateProjectHandler>();
        services.AddTransient<GetProjectsHandler>();

        services.AddSingleton<SqliteResourceRepository>();

        services.AddSingleton<IResourceRepository>(
            provider =>
                provider.GetRequiredService<
                    SqliteResourceRepository>());

        services.AddSingleton<IResourceReader>(
            provider =>
                provider.GetRequiredService<
                    SqliteResourceRepository>());

        services.AddTransient<CreateResourceHandler>();
        services.AddTransient<GetResourcesHandler>();

        services.AddSingleton<SqliteTaskRepository>();

        services.AddSingleton<ITaskRepository>(
            provider =>
                provider.GetRequiredService<
                    SqliteTaskRepository>());

        services.AddSingleton<ITaskReader>(
            provider =>
                provider.GetRequiredService<
                    SqliteTaskRepository>());

        services.AddTransient<CreateTaskHandler>();
        services.AddTransient<GetTasksHandler>();
        services.AddTransient<CompleteTaskHandler>();
        services.AddTransient<ReopenTaskHandler>();

        return services.BuildServiceProvider();
    }

    private static string GetPersonalDataPath()
    {
        var localDataPath =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(
            localDataPath,
            "Lorcaire",
            "PersonalEdition");
    }
}
