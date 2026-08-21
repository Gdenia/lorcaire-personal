using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteProjectRepository :
    IProjectRepository,
    IProjectReader
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteProjectRepository(SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO projects (id, area_id, name, description)
            VALUES ($id, $areaId, $name, $description);
            """;
        command.Parameters.AddWithValue("$id", project.Id.Value.ToString());
        command.Parameters.AddWithValue("$areaId", project.AreaId.Value.ToString());
        command.Parameters.AddWithValue("$name", project.Name);
        command.Parameters.AddWithValue(
            "$description",
            project.Description is null ? DBNull.Value : project.Description);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw SqlitePersistenceErrors.SaveConflict("project", exception);
        }
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, area_id, name, description
            FROM projects
            ORDER BY name COLLATE NOCASE;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var projects = new List<Project>();

        while (await reader.ReadAsync(cancellationToken))
        {
            projects.Add(new Project(
                new ProjectId(Guid.Parse(reader.GetString(0))),
                new AreaId(Guid.Parse(reader.GetString(1))),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return projects;
    }

    public async Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection(); await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT id, area_id, name, description FROM projects WHERE id = $id;"; command.Parameters.AddWithValue("$id", id.Value.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadProject(reader) : null;
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project); await using var connection = _connectionFactory.CreateConnection(); await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand(); command.CommandText = "UPDATE projects SET area_id=$areaId, name=$name, description=$description WHERE id=$id;"; AddParameters(command, project);
        try { if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw SqlitePersistenceErrors.MissingDuringUpdate("project"); }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19) { throw SqlitePersistenceErrors.SaveConflict("project", exception); }
    }

    public async Task<bool> DeleteAsync(ProjectId id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection(); await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand(); command.CommandText = "DELETE FROM projects WHERE id=$id;"; command.Parameters.AddWithValue("$id", id.Value.ToString());
        try { return await command.ExecuteNonQueryAsync(cancellationToken) == 1; }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) { throw SqlitePersistenceErrors.DeleteConflict("project", ex); }
    }

    private static Project ReadProject(SqliteDataReader reader) => new(new ProjectId(Guid.Parse(reader.GetString(0))), new AreaId(Guid.Parse(reader.GetString(1))), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3));
    private static void AddParameters(SqliteCommand command, Project project)
    { command.Parameters.AddWithValue("$id", project.Id.Value.ToString()); command.Parameters.AddWithValue("$areaId", project.AreaId.Value.ToString()); command.Parameters.AddWithValue("$name", project.Name); command.Parameters.AddWithValue("$description", project.Description is null ? DBNull.Value : project.Description); }
}
