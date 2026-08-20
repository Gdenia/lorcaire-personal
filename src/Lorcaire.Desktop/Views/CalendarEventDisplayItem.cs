using Lorcaire.Application.Calendar.GetCalendarEvents;

namespace Lorcaire.Desktop.Views;

public sealed record CalendarEventDisplayItem(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset LocalStartAt,
    DateTimeOffset? LocalEndAt,
    bool IsPast,
    string Schedule)
{
    public static CalendarEventDisplayItem Create(
        CalendarEventSummary summary,
        TimeZoneInfo localTimeZone)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(localTimeZone);

        var localStart = TimeZoneInfo.ConvertTime(
            summary.StartAt,
            localTimeZone);
        DateTimeOffset? localEnd = summary.EndAt is null
            ? null
            : TimeZoneInfo.ConvertTime(
                summary.EndAt.Value,
                localTimeZone);
        var schedule = localEnd is null
            ? localStart.ToString("g")
            : $"{localStart:g} – {localEnd.Value:g}";

        return new CalendarEventDisplayItem(
            summary.Id,
            summary.Title,
            summary.Description,
            summary.StartAt,
            summary.EndAt,
            localStart,
            localEnd,
            summary.IsPast,
            schedule);
    }
}
