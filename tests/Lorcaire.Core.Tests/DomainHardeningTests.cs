using Lorcaire.Core.Domain;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;
using Lorcaire.Core.Domain.Goals;
using Lorcaire.Core.Domain.Notes;
using Lorcaire.Core.Domain.Projects;
using Lorcaire.Core.Domain.Resources;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using TaskId = Lorcaire.Core.Domain.Tasks.TaskId;

namespace Lorcaire.Core.Tests.Domain;

public sealed class DomainHardeningTests
{
    private static readonly DateTimeOffset Start =
        new(2026, 8, 21, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TextLimits_AcceptExactBoundary()
    {
        var areaId = AreaId.New();

        var goal = new Goal(
            GoalId.New(),
            areaId,
            new string('n', DomainTextLimits.NameMaximumLength),
            new string('d', DomainTextLimits.DescriptionMaximumLength));
        var task = new DomainTask(
            TaskId.New(),
            areaId,
            new string('t', DomainTextLimits.TitleMaximumLength));
        var resource = new Resource(
            ResourceId.New(),
            areaId,
            "Resource",
            new string('c', DomainTextLimits.CategoryMaximumLength));
        var note = new Note(
            NoteId.New(),
            areaId,
            "Note",
            new string('x', DomainTextLimits.NoteContentMaximumLength),
            Start);

        Assert.Equal(DomainTextLimits.NameMaximumLength, goal.Name.Length);
        Assert.Equal(DomainTextLimits.DescriptionMaximumLength, goal.Description!.Length);
        Assert.Equal(DomainTextLimits.TitleMaximumLength, task.Title.Length);
        Assert.Equal(DomainTextLimits.CategoryMaximumLength, resource.Category.Length);
        Assert.Equal(DomainTextLimits.NoteContentMaximumLength, note.Content.Length);
    }

    [Fact]
    public void TextLimits_RejectBoundaryPlusOne()
    {
        var areaId = AreaId.New();

        Assert.Throws<ArgumentException>(() => new Goal(
            GoalId.New(),
            areaId,
            new string('n', DomainTextLimits.NameMaximumLength + 1)));
        Assert.Throws<ArgumentException>(() => new Goal(
            GoalId.New(),
            areaId,
            "Goal",
            new string('d', DomainTextLimits.DescriptionMaximumLength + 1)));
        Assert.Throws<ArgumentException>(() => new DomainTask(
            TaskId.New(),
            areaId,
            new string('t', DomainTextLimits.TitleMaximumLength + 1)));
        Assert.Throws<ArgumentException>(() => new Resource(
            ResourceId.New(),
            areaId,
            "Resource",
            new string('c', DomainTextLimits.CategoryMaximumLength + 1)));
        Assert.Throws<ArgumentException>(() => new Note(
            NoteId.New(),
            areaId,
            "Note",
            new string('x', DomainTextLimits.NoteContentMaximumLength + 1),
            Start));
    }

    [Fact]
    public void EntityConstructors_RejectDefaultIdentifiers()
    {
        var areaId = AreaId.New();

        Assert.Throws<ArgumentException>(() =>
            new Goal(default, areaId, "Goal"));
        Assert.Throws<ArgumentException>(() =>
            new Project(default, areaId, "Project"));
        Assert.Throws<ArgumentException>(() =>
            new DomainTask(default, areaId, "Task"));
        Assert.Throws<ArgumentException>(() =>
            new Resource(default, areaId, "Resource", "Book"));
        Assert.Throws<ArgumentException>(() =>
            new CalendarEvent(default, areaId, "Event", Start));
        Assert.Throws<ArgumentException>(() =>
            new Note(default, areaId, "Note", "Content", Start));
        Assert.Throws<ArgumentException>(() =>
            new Goal(GoalId.New(), default, "Goal"));
    }

    [Fact]
    public void GoalUpdate_IsAtomicWhenDescriptionIsInvalid()
    {
        var goal = new Goal(
            GoalId.New(), AreaId.New(), "Old", "Original", isCompleted: true);

        Assert.Throws<ArgumentException>(() => goal.UpdateDetails(
            "New",
            new string('x', DomainTextLimits.DescriptionMaximumLength + 1)));

        Assert.Equal("Old", goal.Name);
        Assert.Equal("Original", goal.Description);
        Assert.True(goal.IsCompleted);
    }

    [Fact]
    public void ProjectAndTaskUpdates_AreAtomicWhenDescriptionIsInvalid()
    {
        var invalidDescription =
            new string('x', DomainTextLimits.DescriptionMaximumLength + 1);
        var project = new Project(
            ProjectId.New(), AreaId.New(), "Old project", "Original");
        var task = new DomainTask(
            TaskId.New(), AreaId.New(), "Old task", "Original", true);

        Assert.Throws<ArgumentException>(() =>
            project.UpdateDetails("New project", invalidDescription));
        Assert.Throws<ArgumentException>(() =>
            task.UpdateDetails("New task", invalidDescription));

        Assert.Equal("Old project", project.Name);
        Assert.Equal("Original", project.Description);
        Assert.Equal("Old task", task.Title);
        Assert.Equal("Original", task.Description);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void ResourceUpdate_IsAtomicWhenThirdFieldIsInvalid()
    {
        var resource = new Resource(
            ResourceId.New(), AreaId.New(), "Old", "Book", "Original");

        Assert.Throws<ArgumentException>(() => resource.UpdateDetails(
            "New",
            "Course",
            new string('x', DomainTextLimits.DescriptionMaximumLength + 1)));

        Assert.Equal("Old", resource.Name);
        Assert.Equal("Book", resource.Category);
        Assert.Equal("Original", resource.Description);
    }

    [Fact]
    public void CalendarUpdate_IsAtomicWhenScheduleIsInvalid()
    {
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            AreaId.New(),
            "Old",
            Start,
            Start.AddHours(1),
            "Original");

        Assert.Throws<ArgumentException>(() => calendarEvent.UpdateDetails(
            "New",
            "Changed",
            Start.AddDays(1),
            Start));

        Assert.Equal("Old", calendarEvent.Title);
        Assert.Equal("Original", calendarEvent.Description);
        Assert.Equal(Start, calendarEvent.StartAt);
        Assert.Equal(Start.AddHours(1), calendarEvent.EndAt);
    }
}
