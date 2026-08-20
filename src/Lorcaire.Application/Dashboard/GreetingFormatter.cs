namespace Lorcaire.Application.Dashboard;

public static class GreetingFormatter
{
    public static string Format(
        string displayName,
        DateTimeOffset localTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var salutation = localTime.Hour switch
        {
            < 12 => "Good morning",
            < 18 => "Good afternoon",
            _ => "Good evening"
        };

        return $"{salutation}, {displayName.Trim()}.";
    }
}
