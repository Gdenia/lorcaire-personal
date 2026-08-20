using Lorcaire.Application.Calendar.Persistence;

namespace Lorcaire.Application.Calendar.GetCalendarEvents;

public sealed class GetCalendarEventsHandler
{
    private readonly ICalendarEventReader _eventReader;
    private readonly TimeProvider _timeProvider;

    public GetCalendarEventsHandler(
        ICalendarEventReader eventReader,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(eventReader);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _eventReader = eventReader;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<CalendarEventSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var events = await _eventReader.GetAllAsync(cancellationToken);
        var now = _timeProvider.GetUtcNow();

        return events
            .Select(calendarEvent => new CalendarEventSummary(
                calendarEvent.Id.Value,
                calendarEvent.AreaId.Value,
                calendarEvent.Title,
                calendarEvent.Description,
                calendarEvent.StartAt,
                calendarEvent.EndAt,
                (calendarEvent.EndAt ?? calendarEvent.StartAt) < now))
            .ToArray();
    }
}
