using Lorcaire.Application.Settings;
using Lorcaire.Infrastructure.Settings;

namespace Lorcaire.Infrastructure.Tests.Settings;

public sealed class JsonUserPreferencesStoreTests
{
    [Fact]
    public async Task LoadAsync_ReturnsDefaults_WhenFileDoesNotExist()
    {
        await using var file = TemporarySettingsFile.Create();
        var store = new JsonUserPreferencesStore(file.FilePath);

        var preferences = await store.LoadAsync();

        Assert.Equal(UserPreferences.Default, preferences);
        Assert.False(File.Exists(file.FilePath));
    }

    [Fact]
    public async Task SaveAndLoad_PersistsPreferencesAcrossInstances()
    {
        await using var file = TemporarySettingsFile.Create();
        var writer = new JsonUserPreferencesStore(file.FilePath);
        var expected = new UserPreferences(
            "Denia",
            AppTheme.Dark,
            showCompletedTasks: false);

        await writer.SaveAsync(expected);

        var reader = new JsonUserPreferencesStore(file.FilePath);
        var stored = await reader.LoadAsync();

        Assert.Equal(expected, stored);
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingPreferences()
    {
        await using var file = TemporarySettingsFile.Create();
        var store = new JsonUserPreferencesStore(file.FilePath);
        await store.SaveAsync(UserPreferences.Default);
        var updated = new UserPreferences(
            "Updated",
            AppTheme.Dark,
            showCompletedTasks: false);

        await store.SaveAsync(updated);

        Assert.Equal(updated, await store.LoadAsync());
        Assert.False(File.Exists(file.FilePath + ".tmp"));
    }

    [Fact]
    public async Task LoadAsync_RejectsMalformedJson()
    {
        await using var file = TemporarySettingsFile.Create();
        Directory.CreateDirectory(file.DirectoryPath);
        await File.WriteAllTextAsync(file.FilePath, "{ invalid json");
        var store = new JsonUserPreferencesStore(file.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => store.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_RejectsUnsupportedSchemaVersion()
    {
        await using var file = TemporarySettingsFile.Create();
        Directory.CreateDirectory(file.DirectoryPath);
        await File.WriteAllTextAsync(
            file.FilePath,
            """
            {
              "SchemaVersion": 999,
              "DisplayName": "User",
              "Theme": 0,
              "ShowCompletedTasks": true
            }
            """);
        var store = new JsonUserPreferencesStore(file.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => store.LoadAsync());
    }

    private sealed class TemporarySettingsFile : IAsyncDisposable
    {
        public string DirectoryPath { get; }
        public string FilePath { get; }

        private TemporarySettingsFile(string directoryPath)
        {
            DirectoryPath = directoryPath;
            FilePath = Path.Combine(directoryPath, "settings.json");
        }

        public static TemporarySettingsFile Create()
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Lorcaire.Tests",
                Guid.NewGuid().ToString("N"));
            return new TemporarySettingsFile(directoryPath);
        }

        public ValueTask DisposeAsync()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }

            return ValueTask.CompletedTask;
        }
    }
}
