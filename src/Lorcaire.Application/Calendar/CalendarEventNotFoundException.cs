namespace Lorcaire.Application.Calendar;
public sealed class CalendarEventNotFoundException(Guid id):Exception($"No calendar event exists with identifier '{id}'.");
