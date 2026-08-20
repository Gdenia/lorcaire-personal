using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lorcaire.Application;
using Lorcaire.Application.Calendar.CreateCalendarEvent;
using Lorcaire.Application.Calendar.GetCalendarEvents;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.GetGoals;
using Lorcaire.Application.Notes.CreateNote;
using Lorcaire.Application.Notes.GetNotes;
using Lorcaire.Application.Notes.UpdateNote;
using Lorcaire.Application.Projects.CreateProject;
using Lorcaire.Application.Projects.GetProjects;
using Lorcaire.Application.Resources.CreateResource;
using Lorcaire.Application.Resources.GetResources;
using Lorcaire.Application.Settings;
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
            var createEventHandler =
                Services.GetRequiredService<CreateCalendarEventHandler>();

            var getEventsHandler =
                Services.GetRequiredService<GetCalendarEventsHandler>();

            var createGoalHandler =
                Services.GetRequiredService<CreateGoalHandler>();

            var getGoalsHandler =
                Services.GetRequiredService<GetGoalsHandler>();

            var createNoteHandler =
                Services.GetRequiredService<CreateNoteHandler>();

            var getNotesHandler =
                Services.GetRequiredService<GetNotesHandler>();

            var updateNoteHandler =
                Services.GetRequiredService<UpdateNoteHandler>();

            var createProjectHandler =
                Services.GetRequiredService<CreateProjectHandler>();

            var getProjectsHandler =
                Services.GetRequiredService<GetProjectsHandler>();

            var createResourceHandler =
                Services.GetRequiredService<CreateResourceHandler>();

            var getResourcesHandler =
                Services.GetRequiredService<GetResourcesHandler>();

            var getPreferencesHandler =
                Services.GetRequiredService<GetUserPreferencesHandler>();

            var savePreferencesHandler =
                Services.GetRequiredService<SaveUserPreferencesHandler>();

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
                createEventHandler,
                getEventsHandler,
                createGoalHandler,
                getGoalsHandler,
                createNoteHandler,
                getNotesHandler,
                updateNoteHandler,
                createProjectHandler,
                getProjectsHandler,
                createResourceHandler,
                getResourcesHandler,
                getPreferencesHandler,
                savePreferencesHandler,
                createTaskHandler,
                getTasksHandler,
                completeTaskHandler,
                reopenTaskHandler,
                workspaceContext);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
