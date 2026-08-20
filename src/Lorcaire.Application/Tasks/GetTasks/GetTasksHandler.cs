using Lorcaire.Application.Tasks.Persistence;

namespace Lorcaire.Application.Tasks.GetTasks;

public sealed class GetTasksHandler
{
    private readonly ITaskReader _taskReader;

    public GetTasksHandler(ITaskReader taskReader) => _taskReader = taskReader;

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var tasks = await _taskReader.GetAllAsync(cancellationToken);

        return tasks
            .Select(task => new TaskSummary(
                task.Id.Value,
                task.AreaId.Value,
                task.Title,
                task.Description,
                task.IsCompleted))
            .ToArray();
    }
}
