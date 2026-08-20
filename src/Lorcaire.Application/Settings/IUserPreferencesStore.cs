namespace Lorcaire.Application.Settings;

public interface IUserPreferencesStore
{
    Task<UserPreferences> LoadAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        UserPreferences preferences,
        CancellationToken cancellationToken = default);
}
