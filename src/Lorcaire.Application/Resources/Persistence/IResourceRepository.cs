using Lorcaire.Core.Domain.Resources;

namespace Lorcaire.Application.Resources.Persistence;

public interface IResourceRepository
{
    Task AddAsync(
        Resource resource,
        CancellationToken cancellationToken = default);
}
