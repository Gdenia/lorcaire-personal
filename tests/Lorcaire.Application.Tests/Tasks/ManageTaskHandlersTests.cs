using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.DeleteTask;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Application.Tasks.UpdateTask;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Tasks;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
namespace Lorcaire.Application.Tests.Tasks;
public sealed class ManageTaskHandlersTests
{
    [Fact]
    public async System.Threading.Tasks.Task UpdatePreservesStatus_AndDeleteReportsMissing()
    {
        var task = new DomainTask(TaskId.New(), AreaId.New(), "Old", isCompleted: true);
        var repository = new Repository(task);
        await new UpdateTaskHandler(repository).HandleAsync(new(task.Id.Value, "New", "Desc"));
        Assert.True(task.IsCompleted);
        Assert.Equal("New", task.Title);
        await new DeleteTaskHandler(repository).HandleAsync(task.Id.Value);
        await Assert.ThrowsAsync<TaskNotFoundException>(() => new DeleteTaskHandler(repository).HandleAsync(task.Id.Value));
    }
    private sealed class Repository(params DomainTask[] values) : ITaskRepository
    {
        private readonly Dictionary<TaskId, DomainTask> _items = values.ToDictionary(x => x.Id);
        public System.Threading.Tasks.Task AddAsync(DomainTask task, CancellationToken c = default) { _items.Add(task.Id, task); return System.Threading.Tasks.Task.CompletedTask; }
        public System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(TaskId id, CancellationToken c = default) { _items.TryGetValue(id, out var task); return System.Threading.Tasks.Task.FromResult(task); }
        public System.Threading.Tasks.Task UpdateAsync(DomainTask task, CancellationToken c = default) { _items[task.Id] = task; return System.Threading.Tasks.Task.CompletedTask; }
        public System.Threading.Tasks.Task<bool> DeleteAsync(TaskId id, CancellationToken c = default) => System.Threading.Tasks.Task.FromResult(_items.Remove(id));
    }
}
