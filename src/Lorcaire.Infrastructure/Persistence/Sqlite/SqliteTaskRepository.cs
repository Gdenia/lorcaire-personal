using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Tasks;
using Microsoft.Data.Sqlite;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteTaskRepository : ITaskRepository, ITaskReader
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteTaskRepository(SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public async System.Threading.Tasks.Task AddAsync(
        DomainTask task,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO tasks (id, area_id, title, description, is_completed)
            VALUES ($id, $areaId, $title, $description, $isCompleted);
            """;
        AddParameters(command, task);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw CreateIntegrityException(exception);
        }
    }

    public async System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(
        TaskId taskId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, area_id, title, description, is_completed
            FROM tasks
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", taskId.Value.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadTask(reader) : null;
    }

    public async System.Threading.Tasks.Task UpdateAsync(
        DomainTask task,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE tasks
            SET area_id = $areaId,
                title = $title,
                description = $description,
                is_completed = $isCompleted
            WHERE id = $id;
            """;
        AddParameters(command, task);
        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affectedRows == 0)
        {
            throw new InvalidOperationException(
                $"No existe una tarea con identificador '{task.Id}'.");
        }
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<DomainTask>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, area_id, title, description, is_completed
            FROM tasks
            ORDER BY is_completed, title COLLATE NOCASE;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var tasks = new List<DomainTask>();
        while (await reader.ReadAsync(cancellationToken))
        {
            tasks.Add(ReadTask(reader));
        }
        return tasks;
    }
    public async System.Threading.Tasks.Task<bool> DeleteAsync(TaskId id,CancellationToken cancellationToken=default)
    { await using var connection=_connectionFactory.CreateConnection();await connection.OpenAsync(cancellationToken);await using var command=connection.CreateCommand();command.CommandText="DELETE FROM tasks WHERE id=$id;";command.Parameters.AddWithValue("$id",id.Value.ToString());try{return await command.ExecuteNonQueryAsync(cancellationToken)==1;}catch(SqliteException ex) when(ex.SqliteErrorCode==19){throw new InvalidOperationException("The task cannot be deleted because other information depends on it.",ex);} }

    private static DomainTask ReadTask(SqliteDataReader reader) =>
        new(
            new TaskId(Guid.Parse(reader.GetString(0))),
            new AreaId(Guid.Parse(reader.GetString(1))),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetInt64(4) == 1);

    private static void AddParameters(
        SqliteCommand command,
        DomainTask task)
    {
        command.Parameters.AddWithValue("$id", task.Id.Value.ToString());
        command.Parameters.AddWithValue("$areaId", task.AreaId.Value.ToString());
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue(
            "$description",
            task.Description is null ? DBNull.Value : task.Description);
        command.Parameters.AddWithValue("$isCompleted", task.IsCompleted ? 1 : 0);
    }

    private static InvalidOperationException CreateIntegrityException(
        SqliteException exception) =>
        new(
            "No se pudo guardar la tarea porque sus datos " +
            "incumplen una restricción de integridad.",
            exception);
}
