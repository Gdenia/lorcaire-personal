using Lorcaire.Application.Errors;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Projects;
namespace Lorcaire.Application.Projects.DeleteProject;

public sealed class DeleteProjectHandler(
    IProjectRepository repository,
    ITaskReader taskReader)
{
    public async Task HandleAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var id = new ProjectId(projectId);
        var tasks = await taskReader.GetAllAsync(cancellationToken);

        if (tasks.Any(task => task.ProjectId == id))
        {
            throw new ConflictException(
                "The project cannot be deleted while tasks are assigned to it.");
        }

        if (!await repository.DeleteAsync(id, cancellationToken))
        {
            throw new ProjectNotFoundException(projectId);
        }
    }
}
