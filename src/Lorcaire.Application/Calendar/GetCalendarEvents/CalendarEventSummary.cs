namespace Lorcaire.Application.Calendar.GetCalendarEvents;

public sealed record CalendarEventSummary(
    Guid Id,
    Guid AreaId,
    string Title,
    string? Description,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt)
{
    public bool IsPast => (EndAt ?? StartAt) < DateTimeOffset.Now;

    public string Schedule =>
        EndAt is null
            ? StartAt.ToString("g")
            : $"{StartAt:g} – {EndAt:g}";
}
