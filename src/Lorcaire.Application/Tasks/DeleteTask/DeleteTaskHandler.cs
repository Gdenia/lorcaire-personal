using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Tasks;
namespace Lorcaire.Application.Tasks.DeleteTask;
public sealed class DeleteTaskHandler(ITaskRepository repository)
{ public async System.Threading.Tasks.Task HandleAsync(Guid taskId,CancellationToken cancellationToken=default){if(!await repository.DeleteAsync(new TaskId(taskId),cancellationToken))throw new TaskNotFoundException(taskId);} }
