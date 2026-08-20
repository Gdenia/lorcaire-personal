namespace Lorcaire.Application.Calendar.CreateCalendarEvent;

public sealed record CreateCalendarEventCommand(
    Guid AreaId,
    string Title,
    string? Description,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt);
