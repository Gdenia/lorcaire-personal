using DomainTask = Lorcaire.Core.Domain.Tasks.Task;

namespace Lorcaire.Application.Tasks.Persistence;

public interface ITaskReader
{
    System.Threading.Tasks.Task<IReadOnlyList<DomainTask>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
