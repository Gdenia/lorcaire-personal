using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Projects;
namespace Lorcaire.Application.Projects.DeleteProject;
public sealed class DeleteProjectHandler(IProjectRepository repository)
{
    public async Task HandleAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        if (!await repository.DeleteAsync(new ProjectId(projectId), cancellationToken))
            throw new ProjectNotFoundException(projectId);
    }
}
