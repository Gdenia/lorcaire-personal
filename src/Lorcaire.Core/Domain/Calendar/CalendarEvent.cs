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
        DomainValidation.EnsureIdentifier(id.Value, "calendar event", nameof(id));
        DomainValidation.EnsureIdentifier(areaId.Value, "area", nameof(areaId));
        Id = id;
        AreaId = areaId;
        Title = ValidateTitle(title);
        ValidateSchedule(startAt, endAt);
        StartAt = startAt.ToUniversalTime();
        EndAt = endAt?.ToUniversalTime();
        Description = NormalizeDescription(description);
    }

    public void Rename(string title) => Title = ValidateTitle(title);

    public void ChangeDescription(string? description) =>
        Description = NormalizeDescription(description);

    public void Reschedule(DateTimeOffset startAt, DateTimeOffset? endAt = null)
    {
        ValidateSchedule(startAt, endAt);
        StartAt = startAt.ToUniversalTime();
        EndAt = endAt?.ToUniversalTime();
    }

    public void UpdateDetails(
        string title,
        string? description,
        DateTimeOffset startAt,
        DateTimeOffset? endAt = null)
    {
        var validatedTitle = ValidateTitle(title);
        var validatedDescription = NormalizeDescription(description);
        ValidateSchedule(startAt, endAt);

        Title = validatedTitle;
        Description = validatedDescription;
        StartAt = startAt.ToUniversalTime();
        EndAt = endAt?.ToUniversalTime();
    }

    private static string ValidateTitle(string title)
    {
        return DomainValidation.RequiredText(
            title,
            DomainTextLimits.TitleMaximumLength,
            "event title",
            nameof(title));
    }

    private static void ValidateSchedule(
        DateTimeOffset startAt,
        DateTimeOffset? endAt)
    {
        if (endAt < startAt)
        {
            throw new ArgumentException(
                "The event end cannot be earlier than its start.",
                nameof(endAt));
        }
    }

    private static string? NormalizeDescription(string? description) =>
        DomainValidation.OptionalText(
            description,
            DomainTextLimits.DescriptionMaximumLength,
            "event description",
            nameof(description));
}
