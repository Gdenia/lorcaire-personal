namespace Lorcaire.Application.Calendar.CreateCalendarEvent;

public sealed class AreaNotFoundException : Exception
{
    public Guid AreaId { get; }

    public AreaNotFoundException(Guid areaId)
        : base($"No existe el área con identificador '{areaId}'.")
    {
        AreaId = areaId;
    }
}
