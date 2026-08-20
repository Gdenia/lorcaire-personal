using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Projects;
namespace Lorcaire.Application.Projects.UpdateProject;
public sealed class UpdateProjectHandler(IProjectRepository repository)
{
    public async Task HandleAsync(UpdateProjectCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var project = await repository.GetByIdAsync(new ProjectId(command.ProjectId), cancellationToken)
            ?? throw new ProjectNotFoundException(command.ProjectId);
        project.Rename(command.Name);
        project.ChangeDescription(command.Description);
        await repository.UpdateAsync(project, cancellationToken);
    }
}
