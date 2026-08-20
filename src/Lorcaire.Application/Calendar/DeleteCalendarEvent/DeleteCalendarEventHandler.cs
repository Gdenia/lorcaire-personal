using Lorcaire.Application.Calendar.Persistence;using Lorcaire.Core.Domain.Calendar;
namespace Lorcaire.Application.Calendar.DeleteCalendarEvent;
public sealed class DeleteCalendarEventHandler(ICalendarEventRepository repository){public async Task HandleAsync(Guid id,CancellationToken c=default){if(!await repository.DeleteAsync(new CalendarEventId(id),c))throw new CalendarEventNotFoundException(id);}}
