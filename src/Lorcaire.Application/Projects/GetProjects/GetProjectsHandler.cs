using Lorcaire.Application.Projects.Persistence;

namespace Lorcaire.Application.Projects.GetProjects;

public sealed class GetProjectsHandler
{
    private readonly IProjectReader _projectReader;

    public GetProjectsHandler(IProjectReader projectReader) =>
        _projectReader = projectReader;

    public async Task<IReadOnlyList<ProjectSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectReader.GetAllAsync(cancellationToken);

        return projects
            .Select(project => new ProjectSummary(
                project.Id.Value,
                project.AreaId.Value,
                project.Name,
                project.Description))
            .ToArray();
    }
}
