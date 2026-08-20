using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using Lorcaire.Core.Domain.Tasks;

namespace Lorcaire.Application.Tasks.CreateTask;

public sealed class CreateTaskHandler
{
    private readonly IAreaRepository _areaRepository;
    private readonly ITaskRepository _taskRepository;

    public CreateTaskHandler(
        IAreaRepository areaRepository,
        ITaskRepository taskRepository)
    {
        _areaRepository = areaRepository;
        _taskRepository = taskRepository;
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

        var task = new DomainTask(
            TaskId.New(),
            areaId,
            command.Title,
            command.Description);

        await _taskRepository.AddAsync(task, cancellationToken);
        return new CreateTaskResult(task.Id.Value);
    }
}
