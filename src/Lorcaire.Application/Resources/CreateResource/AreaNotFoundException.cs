using Lorcaire.Application.Errors;

namespace Lorcaire.Application.Resources.CreateResource;

public sealed class AreaNotFoundException : NotFoundException
{
    public Guid AreaId { get; }

    public AreaNotFoundException(Guid areaId)
        : base($"No area exists with identifier '{areaId}'.")
    {
        AreaId = areaId;
    }
}
