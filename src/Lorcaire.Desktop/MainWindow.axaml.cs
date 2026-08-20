using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application.Settings;
using Lorcaire.Desktop.Composition;

namespace Lorcaire.Desktop;

public partial class MainWindow : Window
{
    private readonly IPageFactory _pageFactory;
    private readonly GetUserPreferencesHandler _getPreferencesHandler;

    public MainWindow(
        IPageFactory pageFactory,
        GetUserPreferencesHandler getPreferencesHandler)
    {
        ArgumentNullException.ThrowIfNull(pageFactory);
        ArgumentNullException.ThrowIfNull(getPreferencesHandler);

        _pageFactory = pageFactory;
        _getPreferencesHandler = getPreferencesHandler;

        InitializeComponent();

        Loaded += LoadPreferences;
        Navigate(DesktopPage.Dashboard);
    }

    private void ShowDashboard(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.Dashboard);

    private void ShowGoals(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.Goals);

    private void ShowProjects(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.Projects);

    private void ShowTasks(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.Tasks);

    private void ShowResources(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.Resources);

    private void ShowCalendar(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.Calendar);

    private void ShowNotes(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.Notes);

    private void ShowSettings(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.Settings);

    private void ShowAbout(object? sender, RoutedEventArgs e) =>
        Navigate(DesktopPage.About);

    private void Navigate(DesktopPage page)
    {
        SetActiveNavigation(GetNavigationButton(page));
        PageTitle.Text = page.ToString();
        PageContent.Content = _pageFactory.Create(
            page,
            Navigate,
            greeting => GreetingText.Text = greeting,
            ApplyPreferences);
    }

    private Button GetNavigationButton(DesktopPage page) =>
        page switch
        {
            DesktopPage.Dashboard => DashboardButton,
            DesktopPage.Goals => GoalsButton,
            DesktopPage.Projects => ProjectsButton,
            DesktopPage.Tasks => TasksButton,
            DesktopPage.Resources => ResourcesButton,
            DesktopPage.Calendar => CalendarButton,
            DesktopPage.Notes => NotesButton,
            DesktopPage.Settings => SettingsButton,
            DesktopPage.About => AboutButton,
            _ => throw new ArgumentOutOfRangeException(nameof(page), page, null)
        };

    private async void LoadPreferences(object? sender, RoutedEventArgs e)
    {
        try
        {
            ApplyPreferences(await _getPreferencesHandler.HandleAsync());
        }
        catch
        {
            ApplyPreferences(UserPreferences.Default);
        }
    }

    private void ApplyPreferences(UserPreferences preferences)
    {
        GreetingText.Text =
            $"Good evening, {preferences.DisplayName}.";
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
}
