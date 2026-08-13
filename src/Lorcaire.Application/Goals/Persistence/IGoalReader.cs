using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Goals.Persistence;

public interface IGoalReader
{
    Task<IReadOnlyList<Goal>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
