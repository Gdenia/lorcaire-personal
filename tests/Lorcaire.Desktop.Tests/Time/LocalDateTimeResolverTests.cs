using Lorcaire.Desktop.Time;

namespace Lorcaire.Desktop.Tests.Time;

public sealed class LocalDateTimeResolverTests
{
    private static readonly TimeZoneInfo TestTimeZone = CreateTimeZone();

    [Fact]
    public void ResolveToUtc_UsesOffsetApplicableToSelectedDate()
    {
        var result = LocalDateTimeResolver.ResolveToUtc(
            new DateTime(2026, 7, 1),
            new TimeSpan(12, 0, 0),
            TestTimeZone);

        Assert.Equal(
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero),
            result);
    }

    [Fact]
    public void ResolveToUtc_RejectsLocalTimeSkippedByDst()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            LocalDateTimeResolver.ResolveToUtc(
                new DateTime(2026, 3, 29),
                new TimeSpan(2, 30, 0),
                TestTimeZone));

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public void ResolveToUtc_AmbiguousTimeSelectsStandardTimeDeterministically()
    {
        var result = LocalDateTimeResolver.ResolveToUtc(
            new DateTime(2026, 10, 25),
            new TimeSpan(2, 30, 0),
            TestTimeZone);

        Assert.Equal(
            new DateTimeOffset(2026, 10, 25, 1, 30, 0, TimeSpan.Zero),
            result);
    }

    private static TimeZoneInfo CreateTimeZone()
    {
        var daylightStart = TimeZoneInfo.TransitionTime.CreateFixedDateRule(
            new DateTime(1, 1, 1, 2, 0, 0),
            3,
            29);
        var daylightEnd = TimeZoneInfo.TransitionTime.CreateFixedDateRule(
            new DateTime(1, 1, 1, 3, 0, 0),
            10,
            25);
        var adjustmentRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31),
            TimeSpan.FromHours(1),
            daylightStart,
            daylightEnd);

        return TimeZoneInfo.CreateCustomTimeZone(
            "Lorcaire Test Time",
            TimeSpan.FromHours(1),
            "Lorcaire Test Time",
            "Lorcaire Test Standard Time",
            "Lorcaire Test Daylight Time",
            [adjustmentRule]);
    }
}
