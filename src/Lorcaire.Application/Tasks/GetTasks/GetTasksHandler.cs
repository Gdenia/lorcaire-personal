using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Tasks.Persistence;

namespace Lorcaire.Application.Tasks.GetTasks;

public sealed class GetTasksHandler
{
    private readonly ITaskReader _taskReader;
    private readonly IProjectReader _projectReader;

    public GetTasksHandler(
        ITaskReader taskReader,
        IProjectReader projectReader)
    {
        _taskReader = taskReader;
        _projectReader = projectReader;
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var tasks = await _taskReader.GetAllAsync(cancellationToken);
        var projects = await _projectReader.GetAllAsync(cancellationToken);
        var projectNames = projects.ToDictionary(
            project => project.Id,
            project => project.Name);

        return tasks
            .Select(task => new TaskSummary(
                task.Id.Value,
                task.AreaId.Value,
                task.Title,
                task.Description,
                task.IsCompleted,
                task.ProjectId?.Value,
                task.ProjectId is { } projectId
                    ? projectNames.GetValueOrDefault(projectId)
                    : null))
            .ToArray();
    }
}
