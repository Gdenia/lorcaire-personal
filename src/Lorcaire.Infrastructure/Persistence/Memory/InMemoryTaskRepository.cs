using System.Collections.Concurrent;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Application.Errors;
using Lorcaire.Core.Domain.Tasks;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;

namespace Lorcaire.Infrastructure.Persistence.Memory;

public sealed class InMemoryTaskRepository : ITaskRepository, ITaskReader
{
    private readonly ConcurrentDictionary<TaskId, DomainTask> _tasks = [];

    public System.Threading.Tasks.Task AddAsync(
        DomainTask task,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_tasks.TryAdd(task.Id, task))
        {
            throw new ConflictException(
                "A task with the same identifier already exists.");
        }

        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(
        TaskId taskId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _tasks.TryGetValue(taskId, out var task);
        return System.Threading.Tasks.Task.FromResult(task);
    }

    public System.Threading.Tasks.Task UpdateAsync(
        DomainTask task,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_tasks.ContainsKey(task.Id))
        {
            throw new ConflictException(
                "The task could not be updated because it no longer exists.");
        }

        _tasks[task.Id] = task;
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task<IReadOnlyList<DomainTask>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<DomainTask> tasks = _tasks.Values
            .OrderBy(task => task.IsCompleted)
            .ThenBy(task => task.Title)
            .ToArray();
        return System.Threading.Tasks.Task.FromResult(tasks);
    }
    public System.Threading.Tasks.Task<bool> DeleteAsync(TaskId id,CancellationToken cancellationToken=default){cancellationToken.ThrowIfCancellationRequested();return System.Threading.Tasks.Task.FromResult(_tasks.TryRemove(id,out _));}
}
