using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lorcaire.Application;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.GetGoals;
using Lorcaire.Application.Projects.CreateProject;
using Lorcaire.Application.Projects.GetProjects;
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

            var workspaceContext =
                Services.GetRequiredService<WorkspaceContext>();

            desktop.MainWindow = new MainWindow(
                createGoalHandler,
                getGoalsHandler,
                createProjectHandler,
                getProjectsHandler,
                workspaceContext);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
