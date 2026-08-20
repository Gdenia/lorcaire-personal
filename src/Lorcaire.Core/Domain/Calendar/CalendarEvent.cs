using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Core.Domain.Calendar;

public sealed class CalendarEvent
{
    public CalendarEventId Id { get; }
    public AreaId AreaId { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset StartAt { get; private set; }
    public DateTimeOffset? EndAt { get; private set; }

    public CalendarEvent(
        CalendarEventId id,
        AreaId areaId,
        string title,
        DateTimeOffset startAt,
        DateTimeOffset? endAt = null,
        string? description = null)
    {
        Id = id;
        AreaId = areaId;
        Title = ValidateTitle(title);
        ValidateSchedule(startAt, endAt);
        StartAt = startAt;
        EndAt = endAt;
        Description = NormalizeDescription(description);
    }

    public void Rename(string title) => Title = ValidateTitle(title);

    public void ChangeDescription(string? description) =>
        Description = NormalizeDescription(description);

    public void Reschedule(DateTimeOffset startAt, DateTimeOffset? endAt = null)
    {
        ValidateSchedule(startAt, endAt);
        StartAt = startAt;
        EndAt = endAt;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "El evento debe tener un título.",
                nameof(title));
        }

        return title.Trim();
    }

    private static void ValidateSchedule(
        DateTimeOffset startAt,
        DateTimeOffset? endAt)
    {
        if (endAt < startAt)
        {
            throw new ArgumentException(
                "La finalización del evento no puede ser anterior al inicio.",
                nameof(endAt));
        }
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
