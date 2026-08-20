using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Tests.Goals.CreateGoal;

public sealed class CreateGoalHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesAndStoresGoal_WhenAreaExists()
    {
        var areaId = Guid.NewGuid();
        var areaRepository = new FakeAreaRepository(areaExists: true);
        var goalRepository = new FakeGoalRepository();

        var handler = new CreateGoalHandler(
            areaRepository,
            goalRepository);

        var command = new CreateGoalCommand(
            areaId,
            "  Mejorar mi salud  ",
            "  Mantener hábitos saludables.  ");

        var result = await handler.HandleAsync(command);

        var storedGoal = Assert.Single(goalRepository.Goals);

        Assert.Equal(result.GoalId, storedGoal.Id.Value);
        Assert.Equal(areaId, storedGoal.AreaId.Value);
        Assert.Equal("Mejorar mi salud", storedGoal.Name);
        Assert.Equal(
            "Mantener hábitos saludables.",
            storedGoal.Description);
    }

    [Fact]
    public async Task HandleAsync_RejectsRequest_WhenAreaDoesNotExist()
    {
        var areaId = Guid.NewGuid();
        var areaRepository = new FakeAreaRepository(areaExists: false);
        var goalRepository = new FakeGoalRepository();

        var handler = new CreateGoalHandler(
            areaRepository,
            goalRepository);

        var command = new CreateGoalCommand(
            areaId,
            "Mejorar mi salud",
            null);

        var exception = await Assert.ThrowsAsync<AreaNotFoundException>(
            () => handler.HandleAsync(command));

        Assert.Equal(areaId, exception.AreaId);
        Assert.Empty(goalRepository.Goals);
    }

    [Fact]
    public async Task HandleAsync_RejectsEmptyAreaId()
    {
        var areaRepository = new FakeAreaRepository(areaExists: true);
        var goalRepository = new FakeGoalRepository();

        var handler = new CreateGoalHandler(
            areaRepository,
            goalRepository);

        var command = new CreateGoalCommand(
            Guid.Empty,
            "Mejorar mi salud",
            null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(command));

        Assert.Empty(goalRepository.Goals);
    }

    [Fact]
    public async Task HandleAsync_DoesNotStoreGoal_WhenNameIsInvalid()
    {
        var areaRepository = new FakeAreaRepository(areaExists: true);
        var goalRepository = new FakeGoalRepository();

        var handler = new CreateGoalHandler(
            areaRepository,
            goalRepository);

        var command = new CreateGoalCommand(
            Guid.NewGuid(),
            "   ",
            null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(command));

        Assert.Empty(goalRepository.Goals);
    }

    private sealed class FakeAreaRepository : IAreaRepository
    {
        private readonly bool _areaExists;

        public FakeAreaRepository(bool areaExists)
        {
            _areaExists = areaExists;
        }

        public Task<bool> ExistsAsync(
            AreaId areaId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_areaExists);
        }
    }

    private sealed class FakeGoalRepository : IGoalRepository
    {
        public List<Goal> Goals { get; } = [];

        public Task AddAsync(
            Goal goal,
            CancellationToken cancellationToken = default)
        {
            Goals.Add(goal);

            return Task.CompletedTask;
        }

        public Task<Goal?> GetByIdAsync(
            GoalId goalId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Goals.SingleOrDefault(goal => goal.Id == goalId));

        public Task UpdateAsync(
            Goal goal,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> DeleteAsync(
            GoalId goalId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Goals.RemoveAll(goal => goal.Id == goalId) == 1);
    }
}
