using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;
using Lorcaire.Infrastructure.Persistence.Sqlite;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteCalendarEventRepositoryTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Repository_PersistsAndReadsEvent()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var writer = new SqliteCalendarEventRepository(
            database.ConnectionFactory);
        var start = new DateTimeOffset(
            2026,
            8,
            21,
            10,
            30,
            0,
            TimeSpan.FromHours(2));
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            database.DefaultAreaId,
            "Review",
            start,
            start.AddHours(1),
            "Description");
        await writer.AddAsync(calendarEvent);

        var reader = new SqliteCalendarEventRepository(
            database.ConnectionFactory);
        var stored = Assert.Single(await reader.GetAllAsync());

        Assert.Equal(calendarEvent.Id, stored.Id);
        Assert.Equal(calendarEvent.AreaId, stored.AreaId);
        Assert.Equal(calendarEvent.Title, stored.Title);
        Assert.Equal(calendarEvent.Description, stored.Description);
        Assert.Equal(calendarEvent.StartAt, stored.StartAt);
        Assert.Equal(calendarEvent.EndAt, stored.EndAt);
        Assert.Equal(TimeSpan.Zero, stored.StartAt.Offset);
        Assert.Equal(TimeSpan.Zero, stored.EndAt!.Value.Offset);

        await using var connection = database.ConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT start_at, end_at FROM calendar_events WHERE id = $id;";
        command.Parameters.AddWithValue("$id", calendarEvent.Id.Value.ToString());
        await using var raw = await command.ExecuteReaderAsync();
        Assert.True(await raw.ReadAsync());
        Assert.EndsWith("+00:00", raw.GetString(0), StringComparison.Ordinal);
        Assert.EndsWith("+00:00", raw.GetString(1), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Repository_OrdersEventsChronologically()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteCalendarEventRepository(
            database.ConnectionFactory);
        var first = new CalendarEvent(
            CalendarEventId.New(),
            database.DefaultAreaId,
            "First",
            new DateTimeOffset(2026, 8, 21, 9, 30, 0, TimeSpan.FromHours(-4)));
        var second = new CalendarEvent(
            CalendarEventId.New(),
            database.DefaultAreaId,
            "Second",
            new DateTimeOffset(2026, 8, 21, 15, 0, 0, TimeSpan.FromHours(2)));
        await repository.AddAsync(second);
        await repository.AddAsync(first);

        var events = await repository.GetAllAsync();

        Assert.Collection(
            events,
            calendarEvent => Assert.Equal(second.Id, calendarEvent.Id),
            calendarEvent => Assert.Equal(first.Id, calendarEvent.Id));
    }

    [Fact]
    public async Task Repository_RejectsUnknownArea()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteCalendarEventRepository(
            database.ConnectionFactory);
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            AreaId.New(),
            "Event",
            Now);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(calendarEvent));
    }

    [Fact]
    public async Task Repository_RejectsDuplicateId()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteCalendarEventRepository(
            database.ConnectionFactory);
        var calendarEvent = new CalendarEvent(
            CalendarEventId.New(),
            database.DefaultAreaId,
            "Event",
            Now);
        await repository.AddAsync(calendarEvent);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(calendarEvent));
    }

    [Fact] public async Task Repository_UpdatesAndDeletes(){await using var database=await TemporaryDatabase.CreateAsync();var r=new SqliteCalendarEventRepository(database.ConnectionFactory);var start=Now.AddDays(1);var x=new CalendarEvent(CalendarEventId.New(),database.DefaultAreaId,"Old",start);await r.AddAsync(x);x.Rename("New");x.Reschedule(start.AddHours(1),start.AddHours(2));await r.UpdateAsync(x);Assert.Equal("New",(await r.GetByIdAsync(x.Id))!.Title);Assert.True(await r.DeleteAsync(x.Id));Assert.False(await r.DeleteAsync(x.Id));}

    private sealed class TemporaryDatabase : IAsyncDisposable
    {
        private readonly string _directoryPath;
        public AreaId DefaultAreaId { get; }
        public SqliteConnectionFactory ConnectionFactory { get; }

        private TemporaryDatabase(
            string directoryPath,
            AreaId defaultAreaId,
            SqliteConnectionFactory connectionFactory)
        {
            _directoryPath = directoryPath;
            DefaultAreaId = defaultAreaId;
            ConnectionFactory = connectionFactory;
        }

        public static async Task<TemporaryDatabase> CreateAsync()
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Lorcaire.Tests",
                Guid.NewGuid().ToString("N"));
            var defaultAreaId = AreaId.New();
            var factory = new SqliteConnectionFactory(
                Path.Combine(directoryPath, "lorcaire-tests.db"));
            await new SqliteDatabaseInitializer(factory)
                .InitializeAsync(defaultAreaId);
            return new TemporaryDatabase(directoryPath, defaultAreaId, factory);
        }

        public ValueTask DisposeAsync()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(_directoryPath, recursive: true);
            }
            return ValueTask.CompletedTask;
        }
    }
}
