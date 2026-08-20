using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Tasks;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;

namespace Lorcaire.Application.Tests.Tasks;

public sealed class ChangeTaskStatusHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task CompleteTask_MarksAndUpdatesTask()
    {
        var task = CreateTask();
        var repository = new FakeTaskRepository(task);

        await new CompleteTaskHandler(repository).HandleAsync(task.Id.Value);

        Assert.True(task.IsCompleted);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReopenTask_MarksAndUpdatesTask()
    {
        var task = CreateTask();
        task.Complete();
        var repository = new FakeTaskRepository(task);

        await new ReopenTaskHandler(repository).HandleAsync(task.Id.Value);

        Assert.False(task.IsCompleted);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async System.Threading.Tasks.Task Handler_RejectsUnknownTask(bool complete)
    {
        var repository = new FakeTaskRepository(null);
        var taskId = Guid.NewGuid();

        if (complete)
        {
            await Assert.ThrowsAsync<TaskNotFoundException>(
                () => new CompleteTaskHandler(repository).HandleAsync(taskId));
        }
        else
        {
            await Assert.ThrowsAsync<TaskNotFoundException>(
                () => new ReopenTaskHandler(repository).HandleAsync(taskId));
        }

        Assert.Equal(0, repository.UpdateCount);
    }

    private static DomainTask CreateTask() =>
        new(TaskId.New(), AreaId.New(), "Tarea");

    private sealed class FakeTaskRepository(
        DomainTask? storedTask) : ITaskRepository
    {
        public int UpdateCount { get; private set; }

        public System.Threading.Tasks.Task AddAsync(
            DomainTask task,
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.CompletedTask;

        public System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(
            TaskId taskId,
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.FromResult(
                storedTask?.Id == taskId ? storedTask : null);

        public System.Threading.Tasks.Task UpdateAsync(
            DomainTask task,
            CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
