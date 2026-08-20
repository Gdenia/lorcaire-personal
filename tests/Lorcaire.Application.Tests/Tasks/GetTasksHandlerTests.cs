using Lorcaire.Application.Tasks.GetTasks;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using Lorcaire.Core.Domain.Tasks;

namespace Lorcaire.Application.Tests.Tasks;

public sealed class GetTasksHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_ReturnsTaskSummaries()
    {
        var task = new DomainTask(
            TaskId.New(),
            AreaId.New(),
            "Tarea",
            "Descripción",
            isCompleted: true);
        var handler = new GetTasksHandler(new FakeTaskReader([task]));

        var summary = Assert.Single(await handler.HandleAsync());

        Assert.Equal(task.Id.Value, summary.Id);
        Assert.Equal(task.AreaId.Value, summary.AreaId);
        Assert.Equal(task.Title, summary.Title);
        Assert.Equal(task.Description, summary.Description);
        Assert.True(summary.IsCompleted);
    }

    private sealed class FakeTaskReader(
        IReadOnlyList<DomainTask> tasks) : ITaskReader
    {
        public System.Threading.Tasks.Task<IReadOnlyList<DomainTask>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.FromResult(tasks);
    }
}
