using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Application.Goals.Persistence;

public interface IAreaRepository
{
    Task<bool> ExistsAsync(
        AreaId areaId,
        CancellationToken cancellationToken = default);
}
