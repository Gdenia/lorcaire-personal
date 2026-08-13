using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Infrastructure.Persistence.Memory;

public sealed class InMemoryAreaRepository : IAreaRepository
{
    private readonly HashSet<AreaId> _areaIds;

    public InMemoryAreaRepository(IEnumerable<AreaId> areaIds)
    {
        ArgumentNullException.ThrowIfNull(areaIds);

        _areaIds = [.. areaIds];
    }

    public Task<bool> ExistsAsync(
        AreaId areaId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_areaIds.Contains(areaId));
    }
}
