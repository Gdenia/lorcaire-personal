using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using Lorcaire.Core.Domain.Tasks;

namespace Lorcaire.Application.Tasks.Persistence;

public interface ITaskRepository
{
    System.Threading.Tasks.Task AddAsync(
        DomainTask task,
        CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(
        TaskId taskId,
        CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task UpdateAsync(
        DomainTask task,
        CancellationToken cancellationToken = default);
}
