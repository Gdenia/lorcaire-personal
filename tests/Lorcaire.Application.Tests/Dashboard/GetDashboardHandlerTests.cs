using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Application.Dashboard;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Application.Settings;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;
using Lorcaire.Core.Domain.Goals;
using Lorcaire.Core.Domain.Notes;
using Lorcaire.Core.Domain.Projects;
using Lorcaire.Core.Domain.Resources;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using TaskId = Lorcaire.Core.Domain.Tasks.TaskId;

namespace Lorcaire.Application.Tests.Dashboard;

public sealed class GetDashboardHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ReturnsHonestEmptyState()
    {
        var summary = await CreateHandler().HandleAsync();

        Assert.Equal("Good morning, User.", summary.Greeting);
        Assert.Equal(0, summary.GoalCount);
        Assert.Equal(0, summary.ActiveGoalCount);
        Assert.Equal(0, summary.ProjectCount);
        Assert.Equal(0, summary.PendingTaskCount);
        Assert.Equal(0, summary.ResourceCount);
        Assert.Empty(summary.PendingTasks);
        Assert.Empty(summary.UpcomingEvents);
        Assert.Empty(summary.RecentActivity);
    }

    [Fact]
    public async Task HandleAsync_AggregatesAndOrdersCurrentData()
    {
        var areaId = AreaId.New();
        var createdNote = new Note(
            NoteId.New(), areaId, "Created", "Body", Now.AddHours(-3));
        var updatedNote = new Note(
            NoteId.New(), areaId, "Updated", "Body", Now.AddDays(-1), Now.AddHours(-1));

        var handler = CreateHandler(
            goals:
            [
                new Goal(GoalId.New(), areaId, "Active"),
                new Goal(GoalId.New(), areaId, "Done", isCompleted: true)
            ],
            projects: [new Project(ProjectId.New(), areaId, "Project")],
            tasks:
            [
                new DomainTask(TaskId.New(), areaId, "Zulu"),
                new DomainTask(TaskId.New(), areaId, "Alpha"),
                new DomainTask(TaskId.New(), areaId, "Completed", isCompleted: true)
            ],
            resources: [new Resource(ResourceId.New(), areaId, "Book", "Reference")],
            events:
            [
                new CalendarEvent(CalendarEventId.New(), areaId, "Later", Now.AddDays(2)),
                new CalendarEvent(CalendarEventId.New(), areaId, "Past", Now.AddMinutes(-1)),
                new CalendarEvent(
                    CalendarEventId.New(),
                    areaId,
                    "Ongoing",
                    Now.AddHours(-1),
                    Now.AddHours(1)),
                new CalendarEvent(CalendarEventId.New(), areaId, "Soon", Now.AddHours(1))
            ],
            notes: [createdNote, updatedNote],
            preferences: new UserPreferences("Alex", AppTheme.Dark, true));

        var summary = await handler.HandleAsync();

        Assert.Equal("Good morning, Alex.", summary.Greeting);
        Assert.Equal(2, summary.GoalCount);
        Assert.Equal(1, summary.ActiveGoalCount);
        Assert.Equal(1, summary.ProjectCount);
        Assert.Equal(2, summary.PendingTaskCount);
        Assert.Equal(1, summary.ResourceCount);
        Assert.Equal(["Alpha", "Zulu"], summary.PendingTasks.Select(task => task.Title));
        Assert.Equal(
            ["Ongoing", "Soon", "Later"],
            summary.UpcomingEvents.Select(item => item.Title));
        Assert.Equal(
            ["Note updated: Updated", "Note created: Created"],
            summary.RecentActivity.Select(item => item.Description));
    }

    [Theory]
    [InlineData(11, "Good morning, Ada.")]
    [InlineData(12, "Good afternoon, Ada.")]
    [InlineData(17, "Good afternoon, Ada.")]
    [InlineData(18, "Good evening, Ada.")]
    public void FormatGreeting_UsesLocalTimeOfDay(int hour, string expected)
    {
        var localTime = new DateTimeOffset(2026, 8, 20, hour, 0, 0, TimeSpan.Zero);

        Assert.Equal(expected, GreetingFormatter.Format("Ada", localTime));
    }

    private static GetDashboardHandler CreateHandler(
        IReadOnlyList<Goal>? goals = null,
        IReadOnlyList<Project>? projects = null,
        IReadOnlyList<DomainTask>? tasks = null,
        IReadOnlyList<Resource>? resources = null,
        IReadOnlyList<CalendarEvent>? events = null,
        IReadOnlyList<Note>? notes = null,
        UserPreferences? preferences = null) =>
        new(
            new FakeGoalReader(goals ?? []),
            new FakeProjectReader(projects ?? []),
            new FakeTaskReader(tasks ?? []),
            new FakeResourceReader(resources ?? []),
            new FakeEventReader(events ?? []),
            new FakeNoteReader(notes ?? []),
            new FakePreferencesStore(preferences ?? UserPreferences.Default),
            new FixedTimeProvider(Now));

    private sealed class FakeGoalReader(IReadOnlyList<Goal> items) : IGoalReader
    {
        public Task<IReadOnlyList<Goal>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class FakeProjectReader(IReadOnlyList<Project> items) : IProjectReader
    {
        public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class FakeTaskReader(IReadOnlyList<DomainTask> items) : ITaskReader
    {
        public Task<IReadOnlyList<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class FakeResourceReader(IReadOnlyList<Resource> items) : IResourceReader
    {
        public Task<IReadOnlyList<Resource>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class FakeEventReader(IReadOnlyList<CalendarEvent> items) : ICalendarEventReader
    {
        public Task<IReadOnlyList<CalendarEvent>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class FakeNoteReader(IReadOnlyList<Note> items) : INoteReader
    {
        public Task<IReadOnlyList<Note>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(items);
    }

    private sealed class FakePreferencesStore(UserPreferences preferences) : IUserPreferencesStore
    {
        public Task<UserPreferences> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(preferences);

        public Task SaveAsync(UserPreferences value, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
