using Lorcaire.Application.Calendar.GetCalendarEvents;
using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Application.Tests.Calendar;

public sealed class GetCalendarEventsHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ReturnsEventSummaries()
    {
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            AreaId.New(),
            "Review",
            Now.AddDays(1),
            description: "Description");
        var handler = new GetCalendarEventsHandler(
            new FakeCalendarEventReader([calendarEvent]),
            new FixedTimeProvider(Now));

        var summary = Assert.Single(await handler.HandleAsync());

        Assert.Equal(calendarEvent.Id.Value, summary.Id);
        Assert.Equal(calendarEvent.AreaId.Value, summary.AreaId);
        Assert.Equal(calendarEvent.Title, summary.Title);
        Assert.Equal(calendarEvent.Description, summary.Description);
        Assert.Equal(calendarEvent.StartAt, summary.StartAt);
        Assert.False(summary.IsPast);
    }

    [Fact]
    public async Task HandleAsync_CalculatesPastStateFromInjectedClock()
    {
        var areaId = AreaId.New();
        var completed = new CalendarEvent(
            CalendarEventId.New(),
            areaId,
            "Completed",
            Now.AddHours(-2),
            Now.AddHours(-1));
        var ongoing = new CalendarEvent(
            CalendarEventId.New(),
            areaId,
            "Ongoing",
            Now.AddHours(-1),
            Now.AddHours(1));
        var upcoming = new CalendarEvent(
            CalendarEventId.New(),
            areaId,
            "Upcoming",
            Now.AddHours(1));
        var handler = new GetCalendarEventsHandler(
            new FakeCalendarEventReader([completed, ongoing, upcoming]),
            new FixedTimeProvider(Now));

        var summaries = await handler.HandleAsync();

        Assert.True(summaries[0].IsPast);
        Assert.False(summaries[1].IsPast);
        Assert.False(summaries[2].IsPast);
    }

    private sealed class FakeCalendarEventReader(
        IReadOnlyList<CalendarEvent> events) : ICalendarEventReader
    {
        public Task<IReadOnlyList<CalendarEvent>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(events);
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
