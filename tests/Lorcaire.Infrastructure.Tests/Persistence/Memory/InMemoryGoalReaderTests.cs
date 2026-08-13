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
}
