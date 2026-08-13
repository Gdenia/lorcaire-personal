using Lorcaire.Application.Goals.GetGoals;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Tests.Goals.GetGoals;

public sealed class GetGoalsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsGoalSummaries()
    {
        var goal = new Goal(
            GoalId.New(),
            AreaId.New(),
            "Mejorar mi salud",
            "Mantener hábitos saludables.");

        var handler = new GetGoalsHandler(
            new FakeGoalReader([goal]));

        var result = await handler.HandleAsync();

        var summary = Assert.Single(result);

        Assert.Equal(goal.Id.Value, summary.Id);
        Assert.Equal(goal.AreaId.Value, summary.AreaId);
        Assert.Equal(goal.Name, summary.Name);
        Assert.Equal(goal.Description, summary.Description);
    }

    private sealed class FakeGoalReader : IGoalReader
    {
        private readonly IReadOnlyList<Goal> _goals;

        public FakeGoalReader(IReadOnlyList<Goal> goals)
        {
            _goals = goals;
        }

        public Task<IReadOnlyList<Goal>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_goals);
        }
    }
}
