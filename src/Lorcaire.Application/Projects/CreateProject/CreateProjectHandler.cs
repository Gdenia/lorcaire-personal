using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Application.Projects.CreateProject;

public sealed class CreateProjectHandler
{
    private readonly IAreaRepository _areaRepository;
    private readonly IProjectRepository _projectRepository;

    public CreateProjectHandler(
        IAreaRepository areaRepository,
        IProjectRepository projectRepository)
    {
        _areaRepository = areaRepository;
        _projectRepository = projectRepository;
    }

    public async Task<CreateProjectResult> HandleAsync(
        CreateProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var areaId = new AreaId(command.AreaId);

        if (!await _areaRepository.ExistsAsync(areaId, cancellationToken))
        {
            throw new AreaNotFoundException(command.AreaId);
        }

        var project = new Project(
            ProjectId.New(),
            areaId,
            command.Name,
            command.Description);

        await _projectRepository.AddAsync(project, cancellationToken);
        return new CreateProjectResult(project.Id.Value);
    }
}
