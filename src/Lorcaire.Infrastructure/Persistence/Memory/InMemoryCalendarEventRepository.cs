using System.Collections.Concurrent;
using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Infrastructure.Persistence.Memory;

public sealed class InMemoryCalendarEventRepository :
    ICalendarEventRepository,
    ICalendarEventReader
{
    private readonly ConcurrentDictionary<CalendarEventId, CalendarEvent>
        _events = [];

    public Task AddAsync(
        CalendarEvent calendarEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_events.TryAdd(calendarEvent.Id, calendarEvent))
        {
            throw new InvalidOperationException(
                $"Ya existe un evento con identificador '{calendarEvent.Id}'.");
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CalendarEvent>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<CalendarEvent> events = _events.Values
            .OrderBy(calendarEvent => calendarEvent.StartAt)
            .ThenBy(calendarEvent => calendarEvent.Title)
            .ToArray();
        return Task.FromResult(events);
    }
    public Task<CalendarEvent?> GetByIdAsync(CalendarEventId id,CancellationToken c=default){c.ThrowIfCancellationRequested();_events.TryGetValue(id,out var x);return Task.FromResult(x);}
    public Task UpdateAsync(CalendarEvent x,CancellationToken c=default){ArgumentNullException.ThrowIfNull(x);c.ThrowIfCancellationRequested();if(!_events.ContainsKey(x.Id))throw new InvalidOperationException($"No existe un evento con identificador '{x.Id}'.");_events[x.Id]=x;return Task.CompletedTask;}
    public Task<bool> DeleteAsync(CalendarEventId id,CancellationToken c=default){c.ThrowIfCancellationRequested();return Task.FromResult(_events.TryRemove(id,out _));}
}
