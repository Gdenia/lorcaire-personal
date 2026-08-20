using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Resources;

namespace Lorcaire.Application.Resources.CreateResource;

public sealed class CreateResourceHandler
{
    private readonly IAreaRepository _areaRepository;
    private readonly IResourceRepository _resourceRepository;

    public CreateResourceHandler(
        IAreaRepository areaRepository,
        IResourceRepository resourceRepository)
    {
        _areaRepository = areaRepository;
        _resourceRepository = resourceRepository;
    }

    public async Task<CreateResourceResult> HandleAsync(
        CreateResourceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var areaId = new AreaId(command.AreaId);

        if (!await _areaRepository.ExistsAsync(areaId, cancellationToken))
        {
            throw new AreaNotFoundException(command.AreaId);
        }

        var resource = new Resource(
            ResourceId.New(),
            areaId,
            command.Name,
            command.Category,
            command.Description);

        await _resourceRepository.AddAsync(resource, cancellationToken);
        return new CreateResourceResult(resource.Id.Value);
    }
}
