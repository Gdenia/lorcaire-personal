using Lorcaire.Application.Calendar.GetCalendarEvents;
using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Application.Tests.Calendar;

public sealed class GetCalendarEventsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsEventSummaries()
    {
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            AreaId.New(),
            "Review",
            DateTimeOffset.Now.AddDays(1),
            description: "Description");
        var handler = new GetCalendarEventsHandler(
            new FakeCalendarEventReader([calendarEvent]));

        var summary = Assert.Single(await handler.HandleAsync());

        Assert.Equal(calendarEvent.Id.Value, summary.Id);
        Assert.Equal(calendarEvent.AreaId.Value, summary.AreaId);
        Assert.Equal(calendarEvent.Title, summary.Title);
        Assert.Equal(calendarEvent.Description, summary.Description);
        Assert.Equal(calendarEvent.StartAt, summary.StartAt);
        Assert.False(summary.IsPast);
    }

    private sealed class FakeCalendarEventReader(
        IReadOnlyList<CalendarEvent> events) : ICalendarEventReader
    {
        public Task<IReadOnlyList<CalendarEvent>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(events);
    }
}
