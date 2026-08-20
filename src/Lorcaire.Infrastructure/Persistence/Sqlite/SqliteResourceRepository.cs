using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Resources;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteResourceRepository :
    IResourceRepository,
    IResourceReader
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteResourceRepository(SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(
        Resource resource,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO resources (id, area_id, name, category, description)
            VALUES ($id, $areaId, $name, $category, $description);
            """;
        command.Parameters.AddWithValue("$id", resource.Id.Value.ToString());
        command.Parameters.AddWithValue("$areaId", resource.AreaId.Value.ToString());
        command.Parameters.AddWithValue("$name", resource.Name);
        command.Parameters.AddWithValue("$category", resource.Category);
        command.Parameters.AddWithValue(
            "$description",
            resource.Description is null ? DBNull.Value : resource.Description);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException(
                "No se pudo guardar el recurso porque sus datos " +
                "incumplen una restricción de integridad.",
                exception);
        }
    }

    public async Task<IReadOnlyList<Resource>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, area_id, name, category, description
            FROM resources
            ORDER BY category COLLATE NOCASE, name COLLATE NOCASE;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var resources = new List<Resource>();

        while (await reader.ReadAsync(cancellationToken))
        {
            resources.Add(new Resource(
                new ResourceId(Guid.Parse(reader.GetString(0))),
                new AreaId(Guid.Parse(reader.GetString(1))),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return resources;
    }
}
