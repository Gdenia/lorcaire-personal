namespace Lorcaire.Application.Settings;

public sealed class SaveUserPreferencesHandler
{
    private readonly IUserPreferencesStore _preferencesStore;

    public SaveUserPreferencesHandler(
        IUserPreferencesStore preferencesStore) =>
        _preferencesStore = preferencesStore;

    public async Task<UserPreferences> HandleAsync(
        SaveUserPreferencesCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var preferences = new UserPreferences(
            command.DisplayName,
            command.Theme,
            command.ShowCompletedTasks);

        await _preferencesStore.SaveAsync(preferences, cancellationToken);
        return preferences;
    }
}
