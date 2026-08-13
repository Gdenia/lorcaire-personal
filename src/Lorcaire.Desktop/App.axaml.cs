using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lorcaire.Application;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.GetGoals;
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

            var workspaceContext =
                Services.GetRequiredService<WorkspaceContext>();

            desktop.MainWindow = new MainWindow(
                createGoalHandler,
                getGoalsHandler,
                workspaceContext);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
