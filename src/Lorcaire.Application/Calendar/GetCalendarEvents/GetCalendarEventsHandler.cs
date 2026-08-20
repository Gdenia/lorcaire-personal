using Lorcaire.Application.Calendar.Persistence;

namespace Lorcaire.Application.Calendar.GetCalendarEvents;

public sealed class GetCalendarEventsHandler
{
    private readonly ICalendarEventReader _eventReader;

    public GetCalendarEventsHandler(ICalendarEventReader eventReader) =>
        _eventReader = eventReader;

    public async Task<IReadOnlyList<CalendarEventSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var events = await _eventReader.GetAllAsync(cancellationToken);

        return events
            .Select(calendarEvent => new CalendarEventSummary(
                calendarEvent.Id.Value,
                calendarEvent.AreaId.Value,
                calendarEvent.Title,
                calendarEvent.Description,
                calendarEvent.StartAt,
                calendarEvent.EndAt))
            .ToArray();
    }
}
