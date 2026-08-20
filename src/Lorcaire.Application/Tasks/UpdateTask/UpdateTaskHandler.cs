using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Tasks;
namespace Lorcaire.Application.Tasks.UpdateTask;
public sealed class UpdateTaskHandler(ITaskRepository repository)
{ public async System.Threading.Tasks.Task HandleAsync(UpdateTaskCommand command,CancellationToken cancellationToken=default){ArgumentNullException.ThrowIfNull(command);var id=new TaskId(command.TaskId);var task=await repository.GetByIdAsync(id,cancellationToken)??throw new TaskNotFoundException(command.TaskId);task.Rename(command.Title);task.ChangeDescription(command.Description);await repository.UpdateAsync(task,cancellationToken);} }
