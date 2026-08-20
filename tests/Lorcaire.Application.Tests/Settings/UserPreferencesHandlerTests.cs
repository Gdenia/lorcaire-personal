using Lorcaire.Application.Settings;

namespace Lorcaire.Application.Tests.Settings;

public sealed class UserPreferencesHandlerTests
{
    [Fact]
    public async Task GetHandler_ReturnsStoredPreferences()
    {
        var expected = new UserPreferences(
            "Denia",
            AppTheme.Dark,
            showCompletedTasks: false);
        var store = new FakePreferencesStore(expected);

        var result = await new GetUserPreferencesHandler(store).HandleAsync();

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task SaveHandler_ValidatesAndPersistsPreferences()
    {
        var store = new FakePreferencesStore(UserPreferences.Default);
        var handler = new SaveUserPreferencesHandler(store);

        var result = await handler.HandleAsync(
            new SaveUserPreferencesCommand(
                "  Denia  ",
                AppTheme.Dark,
                ShowCompletedTasks: false));

        Assert.Equal("Denia", result.DisplayName);
        Assert.False(result.ShowCompletedTasks);
        Assert.Same(result, store.Preferences);
    }

    [Fact]
    public async Task SaveHandler_DoesNotPersistInvalidPreferences()
    {
        var original = UserPreferences.Default;
        var store = new FakePreferencesStore(original);
        var handler = new SaveUserPreferencesHandler(store);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(
                new SaveUserPreferencesCommand(
                    " ",
                    AppTheme.Dark,
                    ShowCompletedTasks: true)));

        Assert.Same(original, store.Preferences);
        Assert.Equal(0, store.SaveCount);
    }

    private sealed class FakePreferencesStore(
        UserPreferences preferences) : IUserPreferencesStore
    {
        public UserPreferences Preferences { get; private set; } = preferences;
        public int SaveCount { get; private set; }

        public Task<UserPreferences> LoadAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Preferences);

        public Task SaveAsync(
            UserPreferences value,
            CancellationToken cancellationToken = default)
        {
            Preferences = value;
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
