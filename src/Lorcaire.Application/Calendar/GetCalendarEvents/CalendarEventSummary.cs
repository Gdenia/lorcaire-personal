namespace Lorcaire.Application.Calendar.GetCalendarEvents;

public sealed record CalendarEventSummary(
    Guid Id,
    Guid AreaId,
    string Title,
    string? Description,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    bool IsPast);
