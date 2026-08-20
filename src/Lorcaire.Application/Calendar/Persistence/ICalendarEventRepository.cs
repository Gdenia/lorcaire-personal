using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Application.Calendar.Persistence;

public interface ICalendarEventRepository
{
    Task AddAsync(
        CalendarEvent calendarEvent,
        CancellationToken cancellationToken = default);
}
