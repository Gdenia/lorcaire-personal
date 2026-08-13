using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.GetGoals;
using Lorcaire.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop;

public partial class MainWindow : Window
{
    private readonly CreateGoalHandler _createGoalHandler;
    private readonly GetGoalsHandler _getGoalsHandler;
    private readonly WorkspaceContext _workspaceContext;

    // Constructor requerido por Avalonia para localizar la ventana
    // mediante su recurso XAML.
    public MainWindow()
        : this(
            App.Services.GetRequiredService<CreateGoalHandler>(),
            App.Services.GetRequiredService<GetGoalsHandler>(),
            App.Services.GetRequiredService<WorkspaceContext>())
    {
    }

    // Constructor principal con dependencias explícitas.
    public MainWindow(
        CreateGoalHandler createGoalHandler,
        GetGoalsHandler getGoalsHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createGoalHandler);
        ArgumentNullException.ThrowIfNull(getGoalsHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createGoalHandler = createGoalHandler;
        _getGoalsHandler = getGoalsHandler;
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
        ShowPlaceholder("Projects");
    }

    private void ShowTasks(
        object? sender,
        RoutedEventArgs e)
    {
        ShowPlaceholder("Tasks");
    }

    private void ShowResources(
        object? sender,
        RoutedEventArgs e)
    {
        ShowPlaceholder("Resources");
    }

    private void ShowCalendar(
        object? sender,
        RoutedEventArgs e)
    {
        ShowPlaceholder("Calendar");
    }

    private void ShowNotes(
        object? sender,
        RoutedEventArgs e)
    {
        ShowPlaceholder("Notes");
    }

    private void ShowSettings(
        object? sender,
        RoutedEventArgs e)
    {
        ShowPlaceholder("Settings");
    }

    private void ShowAbout(
        object? sender,
        RoutedEventArgs e)
    {
        ShowPlaceholder("About");
    }

    private void ShowDashboardView()
    {
        PageTitle.Text = "Dashboard";
        PageContent.Content = new DashboardView();
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
