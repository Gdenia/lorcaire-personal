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
            FormatUtc(calendarEvent.StartAt));
        command.Parameters.AddWithValue(
            "$endAt",
            calendarEvent.EndAt is null
                ? DBNull.Value
                : FormatUtc(calendarEvent.EndAt.Value));

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw SqlitePersistenceErrors.SaveConflict(
                "calendar event",
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
            events.Add(ReadEvent(reader));
        }

        return events;
    }
    public async Task<CalendarEvent?> GetByIdAsync(CalendarEventId id,CancellationToken c=default){await using var connection=_connectionFactory.CreateConnection();await connection.OpenAsync(c);await using var command=connection.CreateCommand();command.CommandText="SELECT id,area_id,title,description,start_at,end_at FROM calendar_events WHERE id=$id;";command.Parameters.AddWithValue("$id",id.Value.ToString());await using var reader=await command.ExecuteReaderAsync(c);return await reader.ReadAsync(c)?ReadEvent(reader):null;}
    public async Task UpdateAsync(CalendarEvent x,CancellationToken c=default){ArgumentNullException.ThrowIfNull(x);await using var connection=_connectionFactory.CreateConnection();await connection.OpenAsync(c);await using var command=connection.CreateCommand();command.CommandText="UPDATE calendar_events SET area_id=$areaId,title=$title,description=$description,start_at=$startAt,end_at=$endAt WHERE id=$id;";AddParameters(command,x);try{if(await command.ExecuteNonQueryAsync(c)==0)throw SqlitePersistenceErrors.MissingDuringUpdate("calendar event");}catch(SqliteException ex)when(ex.SqliteErrorCode==19){throw SqlitePersistenceErrors.SaveConflict("calendar event",ex);}}
    public async Task<bool> DeleteAsync(CalendarEventId id,CancellationToken c=default){await using var connection=_connectionFactory.CreateConnection();await connection.OpenAsync(c);await using var command=connection.CreateCommand();command.CommandText="DELETE FROM calendar_events WHERE id=$id;";command.Parameters.AddWithValue("$id",id.Value.ToString());try{return await command.ExecuteNonQueryAsync(c)==1;}catch(SqliteException ex)when(ex.SqliteErrorCode==19){throw SqlitePersistenceErrors.DeleteConflict("calendar event",ex);}}
    private static CalendarEvent ReadEvent(SqliteDataReader r)=>new(new CalendarEventId(Guid.Parse(r.GetString(0))),new AreaId(Guid.Parse(r.GetString(1))),r.GetString(2),ParseUtc(r.GetString(4)),r.IsDBNull(5)?null:ParseUtc(r.GetString(5)),r.IsDBNull(3)?null:r.GetString(3));
    private static void AddParameters(SqliteCommand c,CalendarEvent x){c.Parameters.AddWithValue("$id",x.Id.Value.ToString());c.Parameters.AddWithValue("$areaId",x.AreaId.Value.ToString());c.Parameters.AddWithValue("$title",x.Title);c.Parameters.AddWithValue("$description",x.Description is null?DBNull.Value:x.Description);c.Parameters.AddWithValue("$startAt",FormatUtc(x.StartAt));c.Parameters.AddWithValue("$endAt",x.EndAt is null?DBNull.Value:FormatUtc(x.EndAt.Value));}
    private static DateTimeOffset ParseUtc(string value)=>DateTimeOffset.Parse(value,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind).ToUniversalTime();
    private static string FormatUtc(DateTimeOffset value)=>value.ToUniversalTime().ToString("O",CultureInfo.InvariantCulture);
}
