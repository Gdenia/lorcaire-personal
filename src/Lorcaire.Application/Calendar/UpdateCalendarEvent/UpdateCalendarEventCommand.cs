namespace Lorcaire.Application.Calendar.UpdateCalendarEvent;
public sealed record UpdateCalendarEventCommand(Guid EventId,string Title,string? Description,DateTimeOffset StartAt,DateTimeOffset? EndAt);
