using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Application.Calendar.Persistence;

public interface ICalendarEventRepository
{
    Task AddAsync(
        CalendarEvent calendarEvent,
        CancellationToken cancellationToken = default);
    Task<CalendarEvent?> GetByIdAsync(CalendarEventId id,CancellationToken cancellationToken=default);
    Task UpdateAsync(CalendarEvent calendarEvent,CancellationToken cancellationToken=default);
    Task<bool> DeleteAsync(CalendarEventId id,CancellationToken cancellationToken=default);
}
