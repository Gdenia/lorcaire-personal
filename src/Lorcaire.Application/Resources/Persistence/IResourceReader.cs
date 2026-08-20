using Lorcaire.Core.Domain.Resources;

namespace Lorcaire.Application.Resources.Persistence;

public interface IResourceReader
{
    Task<IReadOnlyList<Resource>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
