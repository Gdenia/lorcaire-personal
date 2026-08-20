using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;

namespace Lorcaire.Application.Calendar.CreateCalendarEvent;

public sealed class CreateCalendarEventHandler
{
    private readonly IAreaRepository _areaRepository;
    private readonly ICalendarEventRepository _eventRepository;

    public CreateCalendarEventHandler(
        IAreaRepository areaRepository,
        ICalendarEventRepository eventRepository)
    {
        _areaRepository = areaRepository;
        _eventRepository = eventRepository;
    }

    public async Task<CreateCalendarEventResult> HandleAsync(
        CreateCalendarEventCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var areaId = new AreaId(command.AreaId);

        if (!await _areaRepository.ExistsAsync(areaId, cancellationToken))
        {
            throw new AreaNotFoundException(command.AreaId);
        }

        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            areaId,
            command.Title,
            command.StartAt,
            command.EndAt,
            command.Description);

        await _eventRepository.AddAsync(calendarEvent, cancellationToken);
        return new CreateCalendarEventResult(calendarEvent.Id.Value);
    }
}
