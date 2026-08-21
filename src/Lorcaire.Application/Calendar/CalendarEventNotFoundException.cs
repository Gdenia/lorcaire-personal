using Lorcaire.Application.Errors;

namespace Lorcaire.Application.Calendar;
public sealed class CalendarEventNotFoundException(Guid id):NotFoundException($"No calendar event exists with identifier '{id}'.");
