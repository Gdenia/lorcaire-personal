using Lorcaire.Application.Errors;

namespace Lorcaire.Application.Tasks.ChangeTaskStatus;

public sealed class TaskNotFoundException : NotFoundException
{
    public Guid TaskId { get; }

    public TaskNotFoundException(Guid taskId)
        : base($"No task exists with identifier '{taskId}'.")
    {
        TaskId = taskId;
    }
}
