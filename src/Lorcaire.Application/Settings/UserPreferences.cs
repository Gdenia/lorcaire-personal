namespace Lorcaire.Application.Settings;

public sealed record UserPreferences
{
    public const int MaximumDisplayNameLength = 100;

    public static UserPreferences Default { get; } =
        new("User", AppTheme.Dark, showCompletedTasks: true);

    public string DisplayName { get; }
    public AppTheme Theme { get; }
    public bool ShowCompletedTasks { get; }

    public UserPreferences(
        string displayName,
        AppTheme theme,
        bool showCompletedTasks)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "The display name is required.",
                nameof(displayName));
        }

        var normalizedDisplayName = displayName.Trim();

        if (normalizedDisplayName.Length > MaximumDisplayNameLength)
        {
            throw new ArgumentException(
                $"The display name cannot exceed " +
                $"{MaximumDisplayNameLength} characters.",
                nameof(displayName));
        }

        if (theme is not AppTheme.Dark)
        {
            throw new ArgumentOutOfRangeException(
                nameof(theme),
                theme,
                "The requested theme is not available.");
        }

        DisplayName = normalizedDisplayName;
        Theme = theme;
        ShowCompletedTasks = showCompletedTasks;
    }
}
