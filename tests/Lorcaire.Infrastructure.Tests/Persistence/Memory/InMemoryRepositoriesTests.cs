using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;
using Lorcaire.Infrastructure.Persistence.Memory;

namespace Lorcaire.Infrastructure.Tests.Persistence.Memory;

public sealed class InMemoryRepositoriesTests
{
    [Fact]
    public async Task AreaRepository_ReturnsTrue_WhenAreaExists()
    {
        var areaId = AreaId.New();
        var repository = new InMemoryAreaRepository([areaId]);

        var exists = await repository.ExistsAsync(areaId);

        Assert.True(exists);
    }

    [Fact]
    public async Task AreaRepository_ReturnsFalse_WhenAreaDoesNotExist()
    {
        var repository = new InMemoryAreaRepository([]);

        var exists = await repository.ExistsAsync(AreaId.New());

        Assert.False(exists);
    }

    [Fact]
    public async Task GoalRepository_AcceptsNewGoal()
    {
        var repository = new InMemoryGoalRepository();
        var goal = CreateGoal();

        await repository.AddAsync(goal);
    }

    [Fact]
    public async Task GoalRepository_RejectsDuplicatedGoalId()
    {
        var repository = new InMemoryGoalRepository();
        var goal = CreateGoal();

        await repository.AddAsync(goal);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(goal));
    }

    [Fact]
    public async Task Repositories_RespectCancellation()
    {
        var areaRepository = new InMemoryAreaRepository([]);
        var goalRepository = new InMemoryGoalRepository();
        using var cancellation = new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => areaRepository.ExistsAsync(
                AreaId.New(),
                cancellation.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => goalRepository.AddAsync(
                CreateGoal(),
                cancellation.Token));
    }

    private static Goal CreateGoal()
    {
        return new Goal(
            GoalId.New(),
            AreaId.New(),
            "Resultado deseado",
            "Descripción del objetivo.");
    }
}
