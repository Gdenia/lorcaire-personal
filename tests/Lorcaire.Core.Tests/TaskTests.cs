using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Tasks;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;

namespace Lorcaire.Core.Tests.Domain.Tasks;

public sealed class TaskTests
{
    [Fact]
    public void Constructor_CreatesTask_WithNormalizedData()
    {
        var id = TaskId.New();
        var areaId = AreaId.New();
        var task = new DomainTask(
            id,
            areaId,
            "  Preparar entrega  ",
            "  Revisar el resultado.  ");

        Assert.Equal(id, task.Id);
        Assert.Equal(areaId, task.AreaId);
        Assert.Equal("Preparar entrega", task.Title);
        Assert.Equal("Revisar el resultado.", task.Description);
        Assert.False(task.IsCompleted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyTitle(string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new DomainTask(TaskId.New(), AreaId.New(), title));
    }

    [Fact]
    public void Complete_MarksTaskAsCompleted()
    {
        var task = CreateTask();
        task.Complete();
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void Reopen_MarksTaskAsNotCompleted()
    {
        var task = CreateTask();
        task.Complete();
        task.Reopen();
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void Rename_ChangesTitle()
    {
        var task = CreateTask();
        task.Rename("Nuevo título");
        Assert.Equal("Nuevo título", task.Title);
    }

    [Fact]
    public void ChangeDescription_NormalizesEmptyDescriptionToNull()
    {
        var task = CreateTask();
        task.ChangeDescription("  ");
        Assert.Null(task.Description);
    }

    [Fact]
    public void TaskId_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new TaskId(Guid.Empty));
    }

    private static DomainTask CreateTask() =>
        new(TaskId.New(), AreaId.New(), "Tarea", "Descripción");
}
