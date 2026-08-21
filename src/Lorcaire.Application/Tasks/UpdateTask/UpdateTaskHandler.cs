using Lorcaire.Application.Projects;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Projects;
using Lorcaire.Core.Domain.Tasks;
namespace Lorcaire.Application.Tasks.UpdateTask;

public sealed class UpdateTaskHandler(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository)
{
    public async System.Threading.Tasks.Task HandleAsync(
        UpdateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var id = new TaskId(command.TaskId);
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new TaskNotFoundException(command.TaskId);
        var projectId = await ResolveProjectIdAsync(
            command.ProjectId,
            cancellationToken);

        task.UpdateDetails(command.Title, command.Description, projectId);
        await taskRepository.UpdateAsync(task, cancellationToken);
    }

    private async System.Threading.Tasks.Task<ProjectId?> ResolveProjectIdAsync(
        Guid? projectId,
        CancellationToken cancellationToken)
    {
        if (projectId is null)
        {
            return null;
        }

        var id = new ProjectId(projectId.Value);

        if (await projectRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            throw new ProjectNotFoundException(projectId.Value);
        }

        return id;
    }
}
