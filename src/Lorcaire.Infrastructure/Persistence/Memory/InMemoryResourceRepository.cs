using System.Collections.Concurrent;
using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Core.Domain.Resources;

namespace Lorcaire.Infrastructure.Persistence.Memory;

public sealed class InMemoryResourceRepository :
    IResourceRepository,
    IResourceReader
{
    private readonly ConcurrentDictionary<ResourceId, Resource> _resources = [];

    public Task AddAsync(
        Resource resource,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_resources.TryAdd(resource.Id, resource))
        {
            throw new InvalidOperationException(
                $"Ya existe un recurso con identificador '{resource.Id}'.");
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Resource>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Resource> resources = _resources.Values
            .OrderBy(resource => resource.Category)
            .ThenBy(resource => resource.Name)
            .ToArray();
        return Task.FromResult(resources);
    }
}
