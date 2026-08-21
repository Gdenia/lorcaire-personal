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
            throw SqlitePersistenceErrors.SaveConflict("resource", exception);
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
    public async Task<Resource?> GetByIdAsync(ResourceId id,CancellationToken c=default){await using var connection=_connectionFactory.CreateConnection();await connection.OpenAsync(c);await using var command=connection.CreateCommand();command.CommandText="SELECT id,area_id,name,category,description FROM resources WHERE id=$id;";command.Parameters.AddWithValue("$id",id.Value.ToString());await using var reader=await command.ExecuteReaderAsync(c);return await reader.ReadAsync(c)?ReadResource(reader):null;}
    public async Task UpdateAsync(Resource item,CancellationToken c=default){ArgumentNullException.ThrowIfNull(item);await using var connection=_connectionFactory.CreateConnection();await connection.OpenAsync(c);await using var command=connection.CreateCommand();command.CommandText="UPDATE resources SET area_id=$areaId,name=$name,category=$category,description=$description WHERE id=$id;";AddParameters(command,item);try{if(await command.ExecuteNonQueryAsync(c)==0)throw SqlitePersistenceErrors.MissingDuringUpdate("resource");}catch(SqliteException ex)when(ex.SqliteErrorCode==19){throw SqlitePersistenceErrors.SaveConflict("resource",ex);}}
    public async Task<bool> DeleteAsync(ResourceId id,CancellationToken c=default){await using var connection=_connectionFactory.CreateConnection();await connection.OpenAsync(c);await using var command=connection.CreateCommand();command.CommandText="DELETE FROM resources WHERE id=$id;";command.Parameters.AddWithValue("$id",id.Value.ToString());try{return await command.ExecuteNonQueryAsync(c)==1;}catch(SqliteException ex)when(ex.SqliteErrorCode==19){throw SqlitePersistenceErrors.DeleteConflict("resource",ex);}}
    private static Resource ReadResource(SqliteDataReader r)=>new(new ResourceId(Guid.Parse(r.GetString(0))),new AreaId(Guid.Parse(r.GetString(1))),r.GetString(2),r.GetString(3),r.IsDBNull(4)?null:r.GetString(4));
    private static void AddParameters(SqliteCommand c,Resource r){c.Parameters.AddWithValue("$id",r.Id.Value.ToString());c.Parameters.AddWithValue("$areaId",r.AreaId.Value.ToString());c.Parameters.AddWithValue("$name",r.Name);c.Parameters.AddWithValue("$category",r.Category);c.Parameters.AddWithValue("$description",r.Description is null?DBNull.Value:r.Description);}
}
