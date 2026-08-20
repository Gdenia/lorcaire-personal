namespace Lorcaire.Desktop.Time;

internal static class LocalDateTimeResolver
{
    public static DateTimeOffset ResolveToUtc(
        DateTime date,
        TimeSpan time,
        TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        var localDateTime = DateTime.SpecifyKind(
            date.Date.Add(time),
            DateTimeKind.Unspecified);

        if (timeZone.IsInvalidTime(localDateTime))
        {
            throw new InvalidOperationException(
                "The selected local time does not exist because the clocks " +
                "move forward. Choose another time.");
        }

        var offset = timeZone.IsAmbiguousTime(localDateTime)
            ? SelectAmbiguousOffset(timeZone, localDateTime)
            : timeZone.GetUtcOffset(localDateTime);

        return new DateTimeOffset(localDateTime, offset)
            .ToUniversalTime();
    }

    private static TimeSpan SelectAmbiguousOffset(
        TimeZoneInfo timeZone,
        DateTime localDateTime)
    {
        var offsets = timeZone.GetAmbiguousTimeOffsets(localDateTime);

        // Prefer standard time. If a custom zone has no base-offset match,
        // choose the smaller offset, which deterministically selects the
        // later of the two possible instants.
        return offsets.Contains(timeZone.BaseUtcOffset)
            ? timeZone.BaseUtcOffset
            : offsets.Min();
    }
}
