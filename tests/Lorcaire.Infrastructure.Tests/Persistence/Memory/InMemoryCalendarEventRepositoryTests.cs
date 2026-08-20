using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;
using Lorcaire.Infrastructure.Persistence.Memory;

namespace Lorcaire.Infrastructure.Tests.Persistence.Memory;

public sealed class InMemoryCalendarEventRepositoryTests
{
    [Fact]
    public async Task Repository_PersistsAndOrdersEventsChronologically()
    {
        var repository = new InMemoryCalendarEventRepository();
        var areaId = AreaId.New();
        var first = new CalendarEvent(
            CalendarEventId.New(),
            areaId,
            "First",
            DateTimeOffset.Now.AddDays(1));
        var second = new CalendarEvent(
            CalendarEventId.New(),
            areaId,
            "Second",
            DateTimeOffset.Now.AddDays(2));
        await repository.AddAsync(second);
        await repository.AddAsync(first);

        var events = await repository.GetAllAsync();

        Assert.Collection(
            events,
            calendarEvent => Assert.Equal(first.Id, calendarEvent.Id),
            calendarEvent => Assert.Equal(second.Id, calendarEvent.Id));
    }

    [Fact]
    public async Task Repository_RejectsDuplicateId()
    {
        var repository = new InMemoryCalendarEventRepository();
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            AreaId.New(),
            "Event",
            DateTimeOffset.Now);
        await repository.AddAsync(calendarEvent);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(calendarEvent));
    }
}
