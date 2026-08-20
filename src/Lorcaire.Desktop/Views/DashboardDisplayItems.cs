using Lorcaire.Application.Dashboard;

namespace Lorcaire.Desktop.Views;

public sealed record DashboardEventDisplayItem(
    Guid Id,
    string Title,
    string Schedule)
{
    public static DashboardEventDisplayItem Create(
        DashboardEventItem item,
        TimeZoneInfo localTimeZone)
    {
        var start = TimeZoneInfo.ConvertTime(item.StartAt, localTimeZone);
        DateTimeOffset? end = item.EndAt is null
            ? null
            : TimeZoneInfo.ConvertTime(item.EndAt.Value, localTimeZone);

        return new DashboardEventDisplayItem(
            item.Id,
            item.Title,
            end is null
                ? start.ToString("g")
                : $"{start:g} – {end.Value:g}");
    }
}

public sealed record DashboardActivityDisplayItem(
    Guid Id,
    string Description,
    string OccurredAtDisplay)
{
    public static DashboardActivityDisplayItem Create(
        DashboardActivityItem item,
        TimeZoneInfo localTimeZone) =>
        new(
            item.Id,
            item.Description,
            TimeZoneInfo.ConvertTime(item.OccurredAt, localTimeZone)
                .ToString("g"));
}
