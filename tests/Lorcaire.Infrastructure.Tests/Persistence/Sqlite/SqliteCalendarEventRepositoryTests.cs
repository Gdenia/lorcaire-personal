using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Calendar;
using Lorcaire.Infrastructure.Persistence.Sqlite;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteCalendarEventRepositoryTests
{
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
            DateTimeOffset.Now.AddDays(1));
        var second = new CalendarEvent(
            CalendarEventId.New(),
            database.DefaultAreaId,
            "Second",
            DateTimeOffset.Now.AddDays(2));
        await repository.AddAsync(second);
        await repository.AddAsync(first);

        var events = await repository.GetAllAsync();

        Assert.Collection(
            events,
            calendarEvent => Assert.Equal(first.Id, calendarEvent.Id),
            calendarEvent => Assert.Equal(second.Id, calendarEvent.Id));
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
            DateTimeOffset.Now);

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
            DateTimeOffset.Now);
        await repository.AddAsync(calendarEvent);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(calendarEvent));
    }

    [Fact] public async Task Repository_UpdatesAndDeletes(){await using var database=await TemporaryDatabase.CreateAsync();var r=new SqliteCalendarEventRepository(database.ConnectionFactory);var start=DateTimeOffset.UtcNow.AddDays(1);var x=new CalendarEvent(CalendarEventId.New(),database.DefaultAreaId,"Old",start);await r.AddAsync(x);x.Rename("New");x.Reschedule(start.AddHours(1),start.AddHours(2));await r.UpdateAsync(x);Assert.Equal("New",(await r.GetByIdAsync(x.Id))!.Title);Assert.True(await r.DeleteAsync(x.Id));Assert.False(await r.DeleteAsync(x.Id));}

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
