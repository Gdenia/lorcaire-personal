using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteAreaRepository : IAreaRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteAreaRepository(
        SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);

        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ExistsAsync(
        AreaId areaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            SELECT EXISTS
            (
                SELECT 1
                FROM areas
                WHERE id = $areaId
            );
            """;

        command.Parameters.AddWithValue(
            "$areaId",
            areaId.Value.ToString());

        var result = await command.ExecuteScalarAsync(
            cancellationToken);

        return Convert.ToInt64(result) == 1;
    }
}
