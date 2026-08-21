using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;
using Lorcaire.Infrastructure.Persistence.Memory;

namespace Lorcaire.Infrastructure.Tests.Persistence.Memory;

public sealed class InMemoryGoalReaderTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsStoredGoals_OrderedByName()
    {
        var repository = new InMemoryGoalRepository();
        var areaId = AreaId.New();

        var secondGoal = new Goal(
            GoalId.New(),
            areaId,
            "Segundo objetivo");

        var firstGoal = new Goal(
            GoalId.New(),
            areaId,
            "Primer objetivo");

        await repository.AddAsync(secondGoal);
        await repository.AddAsync(firstGoal);

        var goals = await repository.GetAllAsync();

        Assert.Collection(
            goals,
            goal => Assert.Equal("Primer objetivo", goal.Name),
            goal => Assert.Equal("Segundo objetivo", goal.Name));
    }

    [Fact]
    public async Task Repository_UpdatesAndDeletesGoal()
    {
        var repository = new InMemoryGoalRepository();
        var goal = new Goal(GoalId.New(), AreaId.New(), "Original");
        await repository.AddAsync(goal);

        goal.Rename("Updated");
        goal.Complete();
        await repository.UpdateAsync(goal);

        var stored = await repository.GetByIdAsync(goal.Id);
        Assert.NotNull(stored);
        Assert.Equal("Updated", stored.Name);
        Assert.True(stored.IsCompleted);

        Assert.True(await repository.DeleteAsync(goal.Id));
        Assert.False(await repository.DeleteAsync(goal.Id));
        Assert.Null(await repository.GetByIdAsync(goal.Id));
    }

    [Fact]
    public async Task UpdateAsync_RejectsMissingGoal()
    {
        var repository = new InMemoryGoalRepository();
        var goal = new Goal(GoalId.New(), AreaId.New(), "Missing");

        await Assert.ThrowsAsync<ConflictException>(
            () => repository.UpdateAsync(goal));
    }
}
