namespace Lorcaire.Application.Settings;

public sealed class GetUserPreferencesHandler
{
    private readonly IUserPreferencesStore _preferencesStore;

    public GetUserPreferencesHandler(
        IUserPreferencesStore preferencesStore) =>
        _preferencesStore = preferencesStore;

    public Task<UserPreferences> HandleAsync(
        CancellationToken cancellationToken = default) =>
        _preferencesStore.LoadAsync(cancellationToken);
}
