using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Views;

public enum DashboardDestination
{
    Goals,
    Projects,
    Tasks,
    Calendar,
    Notes
}

public partial class DashboardView : UserControl
{
    private readonly GetDashboardHandler _getDashboardHandler;
    private readonly Action<DashboardDestination>? _navigate;
    private readonly Action<string>? _greetingChanged;

    public DashboardView()
        : this(App.Services.GetRequiredService<GetDashboardHandler>())
    {
    }

    public DashboardView(
        GetDashboardHandler getDashboardHandler,
        Action<DashboardDestination>? navigate = null,
        Action<string>? greetingChanged = null)
    {
        ArgumentNullException.ThrowIfNull(getDashboardHandler);

        _getDashboardHandler = getDashboardHandler;
        _navigate = navigate;
        _greetingChanged = greetingChanged;

        InitializeComponent();
        Loaded += LoadDashboard;
    }

    private async void LoadDashboard(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dashboard = await _getDashboardHandler.HandleAsync();

            GoalCount.Text = dashboard.GoalCount.ToString();
            ActiveGoalCount.Text = $"{dashboard.ActiveGoalCount} active";
            ProjectCount.Text = dashboard.ProjectCount.ToString();
            PendingTaskCount.Text = dashboard.PendingTaskCount.ToString();
            ResourceCount.Text = dashboard.ResourceCount.ToString();

            PendingTasksList.ItemsSource = dashboard.PendingTasks;
            UpcomingEventsList.ItemsSource = dashboard.UpcomingEvents;
            RecentActivityList.ItemsSource = dashboard.RecentActivity;

            TasksEmptyState.IsVisible = dashboard.PendingTasks.Count == 0;
            EventsEmptyState.IsVisible = dashboard.UpcomingEvents.Count == 0;
            ActivityEmptyState.IsVisible = dashboard.RecentActivity.Count == 0;

            _greetingChanged?.Invoke(dashboard.Greeting);
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to load dashboard: {exception.Message}";
        }
    }

    private void OpenGoals(object? sender, RoutedEventArgs e) =>
        _navigate?.Invoke(DashboardDestination.Goals);

    private void OpenProjects(object? sender, RoutedEventArgs e) =>
        _navigate?.Invoke(DashboardDestination.Projects);

    private void OpenTasks(object? sender, RoutedEventArgs e) =>
        _navigate?.Invoke(DashboardDestination.Tasks);

    private void OpenCalendar(object? sender, RoutedEventArgs e) =>
        _navigate?.Invoke(DashboardDestination.Calendar);

    private void OpenNotes(object? sender, RoutedEventArgs e) =>
        _navigate?.Invoke(DashboardDestination.Notes);
}
