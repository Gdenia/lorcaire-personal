using System.Globalization;
using Lorcaire.Application.Calendar.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteCalendarEventRepository :
    ICalendarEventRepository,
    ICalendarEventReader
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteCalendarEventRepository(
        SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(
        CalendarEvent calendarEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO calendar_events
                (id, area_id, title, description, start_at, end_at)
            VALUES
                ($id, $areaId, $title, $description, $startAt, $endAt);
            """;
        command.Parameters.AddWithValue(
            "$id",
            calendarEvent.Id.Value.ToString());
        command.Parameters.AddWithValue(
            "$areaId",
            calendarEvent.AreaId.Value.ToString());
        command.Parameters.AddWithValue("$title", calendarEvent.Title);
        command.Parameters.AddWithValue(
            "$description",
            calendarEvent.Description is null
                ? DBNull.Value
                : calendarEvent.Description);
        command.Parameters.AddWithValue(
            "$startAt",
            calendarEvent.StartAt.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue(
            "$endAt",
            calendarEvent.EndAt is null
                ? DBNull.Value
                : calendarEvent.EndAt.Value.ToString(
                    "O",
                    CultureInfo.InvariantCulture));

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException(
                "No se pudo guardar el evento porque sus datos " +
                "incumplen una restricción de integridad.",
                exception);
        }
    }

    public async Task<IReadOnlyList<CalendarEvent>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, area_id, title, description, start_at, end_at
            FROM calendar_events
            ORDER BY start_at, title COLLATE NOCASE;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var events = new List<CalendarEvent>();

        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(new CalendarEvent(
                new CalendarEventId(Guid.Parse(reader.GetString(0))),
                new AreaId(Guid.Parse(reader.GetString(1))),
                reader.GetString(2),
                DateTimeOffset.Parse(
                    reader.GetString(4),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind),
                reader.IsDBNull(5)
                    ? null
                    : DateTimeOffset.Parse(
                        reader.GetString(5),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind),
                reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return events;
    }
}
