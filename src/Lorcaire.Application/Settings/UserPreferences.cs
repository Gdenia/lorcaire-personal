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
                "El nombre mostrado es obligatorio.",
                nameof(displayName));
        }

        var normalizedDisplayName = displayName.Trim();

        if (normalizedDisplayName.Length > MaximumDisplayNameLength)
        {
            throw new ArgumentException(
                $"El nombre mostrado no puede superar " +
                $"{MaximumDisplayNameLength} caracteres.",
                nameof(displayName));
        }

        if (theme is not AppTheme.Dark)
        {
            throw new ArgumentOutOfRangeException(
                nameof(theme),
                theme,
                "El tema solicitado no está disponible.");
        }

        DisplayName = normalizedDisplayName;
        Theme = theme;
        ShowCompletedTasks = showCompletedTasks;
    }
}
