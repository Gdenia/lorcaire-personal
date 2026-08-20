using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lorcaire.Application;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.GetGoals;
using Lorcaire.Application.Projects.CreateProject;
using Lorcaire.Application.Projects.GetProjects;
using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.CreateTask;
using Lorcaire.Application.Tasks.GetTasks;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop;

public partial class App : Avalonia.Application
{
    public static IServiceProvider Services { get; set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var createGoalHandler =
                Services.GetRequiredService<CreateGoalHandler>();

            var getGoalsHandler =
                Services.GetRequiredService<GetGoalsHandler>();

            var createProjectHandler =
                Services.GetRequiredService<CreateProjectHandler>();

            var getProjectsHandler =
                Services.GetRequiredService<GetProjectsHandler>();

            var createTaskHandler =
                Services.GetRequiredService<CreateTaskHandler>();

            var getTasksHandler =
                Services.GetRequiredService<GetTasksHandler>();

            var completeTaskHandler =
                Services.GetRequiredService<CompleteTaskHandler>();

            var reopenTaskHandler =
                Services.GetRequiredService<ReopenTaskHandler>();

            var workspaceContext =
                Services.GetRequiredService<WorkspaceContext>();

            desktop.MainWindow = new MainWindow(
                createGoalHandler,
                getGoalsHandler,
                createProjectHandler,
                getProjectsHandler,
                createTaskHandler,
                getTasksHandler,
                completeTaskHandler,
                reopenTaskHandler,
                workspaceContext);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
