using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Core.Tests.Domain.Calendar;

public sealed class CalendarEventTests
{
    [Fact]
    public void Constructor_CreatesEvent_WithNormalizedData()
    {
        var id = CalendarEventId.New();
        var areaId = AreaId.New();
        var start = new DateTimeOffset(2026, 8, 21, 10, 0, 0, TimeSpan.FromHours(2));
        var end = start.AddHours(1);
        var calendarEvent = new CalendarEvent(
            id,
            areaId,
            "  Project review  ",
            start,
            end,
            "  Review progress.  ");

        Assert.Equal(id, calendarEvent.Id);
        Assert.Equal(areaId, calendarEvent.AreaId);
        Assert.Equal("Project review", calendarEvent.Title);
        Assert.Equal("Review progress.", calendarEvent.Description);
        Assert.Equal(start, calendarEvent.StartAt);
        Assert.Equal(end, calendarEvent.EndAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyTitle(string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new CalendarEvent(
                CalendarEventId.New(),
                AreaId.New(),
                title,
                DateTimeOffset.Now));
    }

    [Fact]
    public void Constructor_AllowsEventWithoutEnd()
    {
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            AreaId.New(),
            "Open event",
            DateTimeOffset.Now);

        Assert.Null(calendarEvent.EndAt);
    }

    [Fact]
    public void Constructor_RejectsEndBeforeStart()
    {
        var start = DateTimeOffset.Now;

        Assert.Throws<ArgumentException>(() =>
            new CalendarEvent(
                CalendarEventId.New(),
                AreaId.New(),
                "Invalid event",
                start,
                start.AddMinutes(-1)));
    }

    [Fact]
    public void Reschedule_RejectsInvalidRange_AndPreservesSchedule()
    {
        var start = DateTimeOffset.Now;
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            AreaId.New(),
            "Event",
            start,
            start.AddHours(1));

        Assert.Throws<ArgumentException>(() =>
            calendarEvent.Reschedule(start.AddDays(1), start));

        Assert.Equal(start, calendarEvent.StartAt);
        Assert.Equal(start.AddHours(1), calendarEvent.EndAt);
    }

    [Fact]
    public void ChangeDescription_NormalizesEmptyValueToNull()
    {
        var calendarEvent = CreateEvent();
        calendarEvent.ChangeDescription(" ");
        Assert.Null(calendarEvent.Description);
    }

    [Fact]
    public void CalendarEventId_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() =>
            new CalendarEventId(Guid.Empty));
    }

    private static CalendarEvent CreateEvent() =>
        new(
            CalendarEventId.New(),
            AreaId.New(),
            "Event",
            DateTimeOffset.Now,
            description: "Description");
}
