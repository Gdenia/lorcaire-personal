using Avalonia.Controls;
using Avalonia.Interactivity;
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
using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.CreateTask;
using Lorcaire.Application.Tasks.GetTasks;
using Lorcaire.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop;

public partial class MainWindow : Window
{
    private readonly CreateGoalHandler _createGoalHandler;
    private readonly CreateCalendarEventHandler _createEventHandler;
    private readonly GetCalendarEventsHandler _getEventsHandler;
    private readonly GetGoalsHandler _getGoalsHandler;
    private readonly CreateNoteHandler _createNoteHandler;
    private readonly GetNotesHandler _getNotesHandler;
    private readonly UpdateNoteHandler _updateNoteHandler;
    private readonly CreateProjectHandler _createProjectHandler;
    private readonly GetProjectsHandler _getProjectsHandler;
    private readonly CreateResourceHandler _createResourceHandler;
    private readonly GetResourcesHandler _getResourcesHandler;
    private readonly CreateTaskHandler _createTaskHandler;
    private readonly GetTasksHandler _getTasksHandler;
    private readonly CompleteTaskHandler _completeTaskHandler;
    private readonly ReopenTaskHandler _reopenTaskHandler;
    private readonly WorkspaceContext _workspaceContext;

    // Constructor requerido por Avalonia para localizar la ventana
    // mediante su recurso XAML.
    public MainWindow()
        : this(
            App.Services.GetRequiredService<CreateCalendarEventHandler>(),
            App.Services.GetRequiredService<GetCalendarEventsHandler>(),
            App.Services.GetRequiredService<CreateGoalHandler>(),
            App.Services.GetRequiredService<GetGoalsHandler>(),
            App.Services.GetRequiredService<CreateNoteHandler>(),
            App.Services.GetRequiredService<GetNotesHandler>(),
            App.Services.GetRequiredService<UpdateNoteHandler>(),
            App.Services.GetRequiredService<CreateProjectHandler>(),
            App.Services.GetRequiredService<GetProjectsHandler>(),
            App.Services.GetRequiredService<CreateResourceHandler>(),
            App.Services.GetRequiredService<GetResourcesHandler>(),
            App.Services.GetRequiredService<CreateTaskHandler>(),
            App.Services.GetRequiredService<GetTasksHandler>(),
            App.Services.GetRequiredService<CompleteTaskHandler>(),
            App.Services.GetRequiredService<ReopenTaskHandler>(),
            App.Services.GetRequiredService<WorkspaceContext>())
    {
    }

    // Constructor principal con dependencias explícitas.
    public MainWindow(
        CreateCalendarEventHandler createEventHandler,
        GetCalendarEventsHandler getEventsHandler,
        CreateGoalHandler createGoalHandler,
        GetGoalsHandler getGoalsHandler,
        CreateNoteHandler createNoteHandler,
        GetNotesHandler getNotesHandler,
        UpdateNoteHandler updateNoteHandler,
        CreateProjectHandler createProjectHandler,
        GetProjectsHandler getProjectsHandler,
        CreateResourceHandler createResourceHandler,
        GetResourcesHandler getResourcesHandler,
        CreateTaskHandler createTaskHandler,
        GetTasksHandler getTasksHandler,
        CompleteTaskHandler completeTaskHandler,
        ReopenTaskHandler reopenTaskHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createEventHandler);
        ArgumentNullException.ThrowIfNull(getEventsHandler);
        ArgumentNullException.ThrowIfNull(createGoalHandler);
        ArgumentNullException.ThrowIfNull(getGoalsHandler);
        ArgumentNullException.ThrowIfNull(createNoteHandler);
        ArgumentNullException.ThrowIfNull(getNotesHandler);
        ArgumentNullException.ThrowIfNull(updateNoteHandler);
        ArgumentNullException.ThrowIfNull(createProjectHandler);
        ArgumentNullException.ThrowIfNull(getProjectsHandler);
        ArgumentNullException.ThrowIfNull(createResourceHandler);
        ArgumentNullException.ThrowIfNull(getResourcesHandler);
        ArgumentNullException.ThrowIfNull(createTaskHandler);
        ArgumentNullException.ThrowIfNull(getTasksHandler);
        ArgumentNullException.ThrowIfNull(completeTaskHandler);
        ArgumentNullException.ThrowIfNull(reopenTaskHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createEventHandler = createEventHandler;
        _getEventsHandler = getEventsHandler;
        _createGoalHandler = createGoalHandler;
        _getGoalsHandler = getGoalsHandler;
        _createNoteHandler = createNoteHandler;
        _getNotesHandler = getNotesHandler;
        _updateNoteHandler = updateNoteHandler;
        _createProjectHandler = createProjectHandler;
        _getProjectsHandler = getProjectsHandler;
        _createResourceHandler = createResourceHandler;
        _getResourcesHandler = getResourcesHandler;
        _createTaskHandler = createTaskHandler;
        _getTasksHandler = getTasksHandler;
        _completeTaskHandler = completeTaskHandler;
        _reopenTaskHandler = reopenTaskHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();

        ShowDashboardView();
    }

    private void ShowDashboard(
        object? sender,
        RoutedEventArgs e)
    {
        ShowDashboardView();
    }

    private void ShowGoals(
        object? sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation(GoalsButton);
        PageTitle.Text = "Goals";

        PageContent.Content = new GoalsView(
            _createGoalHandler,
            _getGoalsHandler,
            _workspaceContext);
    }

    private void ShowProjects(
        object? sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation(ProjectsButton);
        PageTitle.Text = "Projects";

        PageContent.Content = new ProjectsView(
            _createProjectHandler,
            _getProjectsHandler,
            _workspaceContext);
    }

    private void ShowTasks(
        object? sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation(TasksButton);
        PageTitle.Text = "Tasks";

        PageContent.Content = new TasksView(
            _createTaskHandler,
            _getTasksHandler,
            _completeTaskHandler,
            _reopenTaskHandler,
            _workspaceContext);
    }

    private void ShowResources(
        object? sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation(ResourcesButton);
        PageTitle.Text = "Resources";

        PageContent.Content = new ResourcesView(
            _createResourceHandler,
            _getResourcesHandler,
            _workspaceContext);
    }

    private void ShowCalendar(
        object? sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation(CalendarButton);
        PageTitle.Text = "Calendar";

        PageContent.Content = new CalendarView(
            _createEventHandler,
            _getEventsHandler,
            _workspaceContext);
    }

    private void ShowNotes(
        object? sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation(NotesButton);
        PageTitle.Text = "Notes";

        PageContent.Content = new NotesView(
            _createNoteHandler,
            _getNotesHandler,
            _updateNoteHandler,
            _workspaceContext);
    }

    private void ShowSettings(
        object? sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation(SettingsButton);
        ShowPlaceholder("Settings");
    }

    private void ShowAbout(
        object? sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation(AboutButton);
        ShowPlaceholder("About");
    }

    private void ShowDashboardView()
    {
        SetActiveNavigation(DashboardButton);
        PageTitle.Text = "Dashboard";
        PageContent.Content = new DashboardView();
    }

    private void SetActiveNavigation(Button activeButton)
    {
        Button[] navigationButtons =
        [
            DashboardButton,
            GoalsButton,
            ProjectsButton,
            TasksButton,
            ResourcesButton,
            CalendarButton,
            NotesButton,
            SettingsButton,
            AboutButton
        ];

        foreach (var button in navigationButtons)
        {
            button.Classes.Set("Selected", button == activeButton);
        }
    }

    private void ShowPlaceholder(string title)
    {
        PageTitle.Text = title;

        PageContent.Content = new TextBlock
        {
            Text = title,
            FontSize = 26,
            Foreground = Avalonia.Media.Brushes.White
        };
    }
}
