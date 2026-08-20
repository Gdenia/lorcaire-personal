namespace Lorcaire.Application.Tasks.ChangeTaskStatus;

public sealed class TaskNotFoundException : Exception
{
    public Guid TaskId { get; }

    public TaskNotFoundException(Guid taskId)
        : base($"No existe la tarea con identificador '{taskId}'.")
    {
        TaskId = taskId;
    }
}
