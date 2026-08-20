using System.Text.Json;
using Lorcaire.Application.Settings;

namespace Lorcaire.Infrastructure.Settings;

public sealed class JsonUserPreferencesStore : IUserPreferencesStore
{
    private const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            WriteIndented = true
        };

    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonUserPreferencesStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "La ruta del archivo de configuración es obligatoria.",
                nameof(filePath));
        }

        _filePath = Path.GetFullPath(filePath);
    }

    public async Task<UserPreferences> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(_filePath))
            {
                return UserPreferences.Default;
            }

            await using var stream = File.OpenRead(_filePath);
            var document = await JsonSerializer.DeserializeAsync<
                PreferencesDocument>(
                stream,
                SerializerOptions,
                cancellationToken);

            if (document is null)
            {
                throw new InvalidDataException(
                    "El archivo de configuración está vacío.");
            }

            if (document.SchemaVersion != CurrentSchemaVersion)
            {
                throw new InvalidDataException(
                    $"La versión de configuración " +
                    $"'{document.SchemaVersion}' no es compatible.");
            }

            return new UserPreferences(
                document.DisplayName,
                document.Theme,
                document.ShowCompletedTasks);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                "El archivo de configuración no contiene JSON válido.",
                exception);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(
        UserPreferences preferences,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var document = new PreferencesDocument(
                CurrentSchemaVersion,
                preferences.DisplayName,
                preferences.Theme,
                preferences.ShowCompletedTasks);
            var temporaryPath = _filePath + ".tmp";

            try
            {
                await using (var stream = new FileStream(
                    temporaryPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await JsonSerializer.SerializeAsync(
                        stream,
                        document,
                        SerializerOptions,
                        cancellationToken);
                }

                File.Move(temporaryPath, _filePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private sealed record PreferencesDocument(
        int SchemaVersion,
        string DisplayName,
        AppTheme Theme,
        bool ShowCompletedTasks);
}
