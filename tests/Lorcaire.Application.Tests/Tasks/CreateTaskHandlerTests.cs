using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Tasks.CreateTask;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Tasks;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;

namespace Lorcaire.Application.Tests.Tasks;

public sealed class CreateTaskHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_CreatesAndStoresTask()
    {
        var repository = new FakeTaskRepository();
        var areaId = Guid.NewGuid();
        var handler = new CreateTaskHandler(
            new FakeAreaRepository(true),
            repository);

        var result = await handler.HandleAsync(
            new CreateTaskCommand(areaId, "Tarea", "Descripción"));

        var task = Assert.Single(repository.Tasks);
        Assert.Equal(result.TaskId, task.Id.Value);
        Assert.Equal(areaId, task.AreaId.Value);
        Assert.Equal("Tarea", task.Title);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_RejectsUnknownArea()
    {
        var repository = new FakeTaskRepository();
        var handler = new CreateTaskHandler(
            new FakeAreaRepository(false),
            repository);

        await Assert.ThrowsAsync<AreaNotFoundException>(() =>
            handler.HandleAsync(
                new CreateTaskCommand(Guid.NewGuid(), "Tarea", null)));
        Assert.Empty(repository.Tasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_RejectsInvalidTitle()
    {
        var repository = new FakeTaskRepository();
        var handler = new CreateTaskHandler(
            new FakeAreaRepository(true),
            repository);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(
                new CreateTaskCommand(Guid.NewGuid(), " ", null)));
        Assert.Empty(repository.Tasks);
    }

    private sealed class FakeAreaRepository(bool exists) : IAreaRepository
    {
        public System.Threading.Tasks.Task<bool> ExistsAsync(
            AreaId areaId,
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.FromResult(exists);
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        public List<DomainTask> Tasks { get; } = [];

        public System.Threading.Tasks.Task AddAsync(
            DomainTask task,
            CancellationToken cancellationToken = default)
        {
            Tasks.Add(task);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(
            TaskId taskId,
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.FromResult(
                Tasks.SingleOrDefault(task => task.Id == taskId));

        public System.Threading.Tasks.Task UpdateAsync(
            DomainTask task,
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.CompletedTask;
    }
}
