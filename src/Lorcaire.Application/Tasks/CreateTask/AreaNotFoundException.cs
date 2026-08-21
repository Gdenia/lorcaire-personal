using Lorcaire.Application.Errors;

namespace Lorcaire.Application.Tasks.CreateTask;

public sealed class AreaNotFoundException : NotFoundException
{
    public Guid AreaId { get; }

    public AreaNotFoundException(Guid areaId)
        : base($"No area exists with identifier '{areaId}'.")
    {
        AreaId = areaId;
    }
}
