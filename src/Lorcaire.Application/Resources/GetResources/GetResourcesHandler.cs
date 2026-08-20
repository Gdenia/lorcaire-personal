using Lorcaire.Application.Resources.Persistence;

namespace Lorcaire.Application.Resources.GetResources;

public sealed class GetResourcesHandler
{
    private readonly IResourceReader _resourceReader;

    public GetResourcesHandler(IResourceReader resourceReader) =>
        _resourceReader = resourceReader;

    public async Task<IReadOnlyList<ResourceSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var resources = await _resourceReader.GetAllAsync(cancellationToken);

        return resources
            .Select(resource => new ResourceSummary(
                resource.Id.Value,
                resource.AreaId.Value,
                resource.Name,
                resource.Category,
                resource.Description))
            .ToArray();
    }
}
