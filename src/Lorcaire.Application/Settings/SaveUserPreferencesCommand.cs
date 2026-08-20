namespace Lorcaire.Application.Settings;

public sealed record SaveUserPreferencesCommand(
    string DisplayName,
    AppTheme Theme,
    bool ShowCompletedTasks);
