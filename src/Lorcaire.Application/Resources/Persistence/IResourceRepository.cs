using Lorcaire.Core.Domain.Resources;

namespace Lorcaire.Application.Resources.Persistence;

public interface IResourceRepository
{
    Task AddAsync(
        Resource resource,
        CancellationToken cancellationToken = default);
    Task<Resource?> GetByIdAsync(ResourceId id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Resource resource, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(ResourceId id, CancellationToken cancellationToken = default);
}
