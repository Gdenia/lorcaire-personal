using Lorcaire.Application.Calendar.CreateCalendarEvent;
using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Application.Tests.Calendar;

public sealed class CreateCalendarEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesAndStoresEvent()
    {
        var repository = new FakeCalendarEventRepository();
        var areaId = Guid.NewGuid();
        var start = DateTimeOffset.Now.AddDays(1);
        var end = start.AddHours(2);
        var handler = new CreateCalendarEventHandler(
            new FakeAreaRepository(true),
            repository);

        var result = await handler.HandleAsync(
            new CreateCalendarEventCommand(
                areaId,
                "Review",
                "Description",
                start,
                end));

        var calendarEvent = Assert.Single(repository.Events);
        Assert.Equal(result.CalendarEventId, calendarEvent.Id.Value);
        Assert.Equal(areaId, calendarEvent.AreaId.Value);
        Assert.Equal(start, calendarEvent.StartAt);
        Assert.Equal(end, calendarEvent.EndAt);
    }

    [Fact]
    public async Task HandleAsync_RejectsUnknownArea()
    {
        var repository = new FakeCalendarEventRepository();
        var handler = new CreateCalendarEventHandler(
            new FakeAreaRepository(false),
            repository);

        await Assert.ThrowsAsync<AreaNotFoundException>(() =>
            handler.HandleAsync(
                new CreateCalendarEventCommand(
                    Guid.NewGuid(),
                    "Review",
                    null,
                    DateTimeOffset.Now,
                    null)));
        Assert.Empty(repository.Events);
    }

    [Fact]
    public async Task HandleAsync_DoesNotStoreInvalidTemporalRange()
    {
        var repository = new FakeCalendarEventRepository();
        var handler = new CreateCalendarEventHandler(
            new FakeAreaRepository(true),
            repository);
        var start = DateTimeOffset.Now;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(
                new CreateCalendarEventCommand(
                    Guid.NewGuid(),
                    "Review",
                    null,
                    start,
                    start.AddMinutes(-1))));
        Assert.Empty(repository.Events);
    }

    private sealed class FakeAreaRepository(bool exists) : IAreaRepository
    {
        public Task<bool> ExistsAsync(
            AreaId areaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);
    }

    private sealed class FakeCalendarEventRepository :
        ICalendarEventRepository
    {
        public List<CalendarEvent> Events { get; } = [];

        public Task AddAsync(
            CalendarEvent calendarEvent,
            CancellationToken cancellationToken = default)
        {
            Events.Add(calendarEvent);
            return Task.CompletedTask;
        }
        public Task<CalendarEvent?> GetByIdAsync(CalendarEventId id,CancellationToken c=default)=>Task.FromResult(Events.SingleOrDefault(x=>x.Id==id));
        public Task UpdateAsync(CalendarEvent item,CancellationToken c=default)=>Task.CompletedTask;
        public Task<bool> DeleteAsync(CalendarEventId id,CancellationToken c=default)=>Task.FromResult(Events.RemoveAll(x=>x.Id==id)==1);
    }
}
