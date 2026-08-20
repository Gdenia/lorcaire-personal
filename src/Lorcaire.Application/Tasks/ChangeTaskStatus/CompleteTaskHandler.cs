using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Tasks;

namespace Lorcaire.Application.Tasks.ChangeTaskStatus;

public sealed class CompleteTaskHandler
{
    private readonly ITaskRepository _taskRepository;

    public CompleteTaskHandler(ITaskRepository taskRepository) =>
        _taskRepository = taskRepository;

    public async System.Threading.Tasks.Task HandleAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var id = new TaskId(taskId);
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new TaskNotFoundException(taskId);

        task.Complete();
        await _taskRepository.UpdateAsync(task, cancellationToken);
    }
}
