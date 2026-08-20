using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Application.Settings;
using Lorcaire.Application.Tasks.Persistence;

namespace Lorcaire.Application.Dashboard;

public sealed class GetDashboardHandler
{
    private const int MaximumPendingTasks = 5;
    private const int MaximumUpcomingEvents = 5;
    private const int MaximumRecentActivity = 5;

    private readonly IGoalReader _goalReader;
    private readonly IProjectReader _projectReader;
    private readonly ITaskReader _taskReader;
    private readonly IResourceReader _resourceReader;
    private readonly ICalendarEventReader _eventReader;
    private readonly INoteReader _noteReader;
    private readonly IUserPreferencesStore _preferencesStore;
    private readonly TimeProvider _timeProvider;

    public GetDashboardHandler(
        IGoalReader goalReader,
        IProjectReader projectReader,
        ITaskReader taskReader,
        IResourceReader resourceReader,
        ICalendarEventReader eventReader,
        INoteReader noteReader,
        IUserPreferencesStore preferencesStore,
        TimeProvider timeProvider)
    {
        _goalReader = goalReader;
        _projectReader = projectReader;
        _taskReader = taskReader;
        _resourceReader = resourceReader;
        _eventReader = eventReader;
        _noteReader = noteReader;
        _preferencesStore = preferencesStore;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardSummary> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var goalsTask = _goalReader.GetAllAsync(cancellationToken);
        var projectsTask = _projectReader.GetAllAsync(cancellationToken);
        var tasksTask = _taskReader.GetAllAsync(cancellationToken);
        var resourcesTask = _resourceReader.GetAllAsync(cancellationToken);
        var eventsTask = _eventReader.GetAllAsync(cancellationToken);
        var notesTask = _noteReader.GetAllAsync(cancellationToken);
        var preferencesTask = _preferencesStore.LoadAsync(cancellationToken);

        await Task.WhenAll(
            goalsTask,
            projectsTask,
            tasksTask,
            resourcesTask,
            eventsTask,
            notesTask,
            preferencesTask);

        var goals = await goalsTask;
        var projects = await projectsTask;
        var tasks = await tasksTask;
        var resources = await resourcesTask;
        var events = await eventsTask;
        var notes = await notesTask;
        var preferences = await preferencesTask;
        var now = _timeProvider.GetUtcNow();

        var pendingTasks = tasks
            .Where(task => !task.IsCompleted)
            .OrderBy(task => task.Title)
            .Select(task => new DashboardTaskItem(
                task.Id.Value,
                task.Title,
                task.Description))
            .ToArray();

        var upcomingEvents = events
            .Where(calendarEvent =>
                (calendarEvent.EndAt ?? calendarEvent.StartAt) >= now)
            .OrderBy(calendarEvent => calendarEvent.StartAt)
            .ThenBy(calendarEvent => calendarEvent.Title)
            .Take(MaximumUpcomingEvents)
            .Select(calendarEvent => new DashboardEventItem(
                calendarEvent.Id.Value,
                calendarEvent.Title,
                calendarEvent.StartAt,
                calendarEvent.EndAt))
            .ToArray();

        var recentActivity = notes
            .OrderByDescending(note => note.LastModifiedAt)
            .ThenBy(note => note.Title)
            .Take(MaximumRecentActivity)
            .Select(note => new DashboardActivityItem(
                note.Id.Value,
                note.CreatedAt == note.LastModifiedAt
                    ? $"Note created: {note.Title}"
                    : $"Note updated: {note.Title}",
                note.LastModifiedAt))
            .ToArray();

        return new DashboardSummary(
            GreetingFormatter.Format(
                preferences.DisplayName,
                _timeProvider.GetLocalNow()),
            goals.Count,
            goals.Count(goal => !goal.IsCompleted),
            projects.Count,
            pendingTasks.Length,
            resources.Count,
            pendingTasks.Take(MaximumPendingTasks).ToArray(),
            upcomingEvents,
            recentActivity);
    }
}
