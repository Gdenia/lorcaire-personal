using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Application.Projects;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using Lorcaire.Core.Domain.Tasks;

namespace Lorcaire.Application.Tasks.CreateTask;

public sealed class CreateTaskHandler
{
    private readonly IAreaRepository _areaRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public CreateTaskHandler(
        IAreaRepository areaRepository,
        ITaskRepository taskRepository,
        IProjectRepository projectRepository)
    {
        _areaRepository = areaRepository;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<CreateTaskResult> HandleAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var areaId = new AreaId(command.AreaId);

        if (!await _areaRepository.ExistsAsync(areaId, cancellationToken))
        {
            throw new AreaNotFoundException(command.AreaId);
        }

        var projectId = await ResolveProjectIdAsync(
            command.ProjectId,
            cancellationToken);

        var task = new DomainTask(
            TaskId.New(),
            areaId,
            command.Title,
            command.Description,
            projectId: projectId);

        await _taskRepository.AddAsync(task, cancellationToken);
        return new CreateTaskResult(task.Id.Value);
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
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            throw new ProjectNotFoundException(projectId.Value);
        }

        return id;
    }
}
