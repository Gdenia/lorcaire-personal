using System.Globalization;
using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Notes;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteNoteRepository : INoteRepository, INoteReader
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteNoteRepository(SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(
        Note note,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO notes
                (id, area_id, title, content, created_at, last_modified_at)
            VALUES
                ($id, $areaId, $title, $content, $createdAt, $lastModifiedAt);
            """;
        AddParameters(command, note);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw CreateIntegrityException(exception);
        }
    }

    public async Task<Note?> GetByIdAsync(
        NoteId noteId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, area_id, title, content, created_at, last_modified_at
            FROM notes
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", noteId.Value.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadNote(reader) : null;
    }

    public async Task UpdateAsync(
        Note note,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE notes
            SET area_id = $areaId,
                title = $title,
                content = $content,
                created_at = $createdAt,
                last_modified_at = $lastModifiedAt
            WHERE id = $id;
            """;
        AddParameters(command, note);
        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affectedRows == 0)
        {
            throw new InvalidOperationException(
                $"No existe una nota con identificador '{note.Id}'.");
        }
    }

    public async Task<IReadOnlyList<Note>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, area_id, title, content, created_at, last_modified_at
            FROM notes
            ORDER BY last_modified_at DESC, title COLLATE NOCASE;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var notes = new List<Note>();
        while (await reader.ReadAsync(cancellationToken))
        {
            notes.Add(ReadNote(reader));
        }
        return notes;
    }
    public async Task<bool> DeleteAsync(NoteId id,CancellationToken c=default){await using var connection=_connectionFactory.CreateConnection();await connection.OpenAsync(c);await using var command=connection.CreateCommand();command.CommandText="DELETE FROM notes WHERE id=$id;";command.Parameters.AddWithValue("$id",id.Value.ToString());try{return await command.ExecuteNonQueryAsync(c)==1;}catch(SqliteException ex)when(ex.SqliteErrorCode==19){throw new InvalidOperationException("The note cannot be deleted because other information depends on it.",ex);}}

    private static Note ReadNote(SqliteDataReader reader) =>
        new(
            new NoteId(Guid.Parse(reader.GetString(0))),
            new AreaId(Guid.Parse(reader.GetString(1))),
            reader.GetString(2),
            reader.GetString(3),
            ParseTimestamp(reader.GetString(4)),
            ParseTimestamp(reader.GetString(5)));

    private static DateTimeOffset ParseTimestamp(string value) =>
        DateTimeOffset.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

    private static void AddParameters(SqliteCommand command, Note note)
    {
        command.Parameters.AddWithValue("$id", note.Id.Value.ToString());
        command.Parameters.AddWithValue("$areaId", note.AreaId.Value.ToString());
        command.Parameters.AddWithValue("$title", note.Title);
        command.Parameters.AddWithValue("$content", note.Content);
        command.Parameters.AddWithValue(
            "$createdAt",
            note.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue(
            "$lastModifiedAt",
            note.LastModifiedAt.ToString("O", CultureInfo.InvariantCulture));
    }

    private static InvalidOperationException CreateIntegrityException(
        SqliteException exception) =>
        new(
            "No se pudo guardar la nota porque sus datos " +
            "incumplen una restricción de integridad.",
            exception);
}
