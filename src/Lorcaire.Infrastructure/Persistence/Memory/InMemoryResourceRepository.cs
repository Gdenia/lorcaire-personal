using System.Collections.Concurrent;
using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Application.Errors;
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
            throw new ConflictException(
                "A resource with the same identifier already exists.");
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
    public Task<Resource?> GetByIdAsync(ResourceId id,CancellationToken c=default){c.ThrowIfCancellationRequested();_resources.TryGetValue(id,out var item);return Task.FromResult(item);}
    public Task UpdateAsync(Resource item,CancellationToken c=default){ArgumentNullException.ThrowIfNull(item);c.ThrowIfCancellationRequested();if(!_resources.ContainsKey(item.Id))throw new ConflictException("The resource could not be updated because it no longer exists.");_resources[item.Id]=item;return Task.CompletedTask;}
    public Task<bool> DeleteAsync(ResourceId id,CancellationToken c=default){c.ThrowIfCancellationRequested();return Task.FromResult(_resources.TryRemove(id,out _));}
}
