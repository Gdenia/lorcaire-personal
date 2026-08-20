using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Application.Calendar.Persistence;

public interface ICalendarEventReader
{
    Task<IReadOnlyList<CalendarEvent>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
