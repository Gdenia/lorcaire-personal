using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Tasks.Persistence;

namespace Lorcaire.Application.Projects.GetProjects;

public sealed class GetProjectsHandler
{
    private readonly IProjectReader _projectReader;
    private readonly ITaskReader _taskReader;

    public GetProjectsHandler(
        IProjectReader projectReader,
        ITaskReader taskReader)
    {
        _projectReader = projectReader;
        _taskReader = taskReader;
    }

    public async Task<IReadOnlyList<ProjectSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectReader.GetAllAsync(cancellationToken);
        var tasks = await _taskReader.GetAllAsync(cancellationToken);
        var taskCounts = tasks
            .Where(task => task.ProjectId is not null)
            .GroupBy(task => task.ProjectId!.Value)
            .ToDictionary(group => group.Key, group => group.Count());

        return projects
            .Select(project => new ProjectSummary(
                project.Id.Value,
                project.AreaId.Value,
                project.Name,
                project.Description,
                taskCounts.GetValueOrDefault(project.Id)))
            .ToArray();
    }
}
