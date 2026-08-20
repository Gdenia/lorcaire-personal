using Lorcaire.Application;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.GetGoals;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Projects.CreateProject;
using Lorcaire.Application.Projects.GetProjects;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Infrastructure.Persistence.Sqlite;
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

        var databasePath = GetDatabasePath();

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

        return services.BuildServiceProvider();
    }

    private static string GetDatabasePath()
    {
        var localDataPath =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(
            localDataPath,
            "Lorcaire",
            "PersonalEdition",
            "lorcaire.db");
    }
}
