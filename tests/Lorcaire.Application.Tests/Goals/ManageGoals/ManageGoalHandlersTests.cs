using Lorcaire.Application.Goals;
using Lorcaire.Application.Goals.ChangeGoalStatus;
using Lorcaire.Application.Goals.DeleteGoal;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Goals.UpdateGoal;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;
using Lorcaire.Core.Domain;

namespace Lorcaire.Application.Tests.Goals.ManageGoals;

public sealed class ManageGoalHandlersTests
{
    [Fact]
    public async Task Update_ChangesNameAndDescription_AndPreservesCompletion()
    {
        var goal = CreateGoal(isCompleted: true);
        var repository = new FakeGoalRepository(goal);

        await new UpdateGoalHandler(repository).HandleAsync(
            new UpdateGoalCommand(goal.Id.Value, "  Updated  ", " New description "));

        Assert.Equal("Updated", goal.Name);
        Assert.Equal("New description", goal.Description);
        Assert.True(goal.IsCompleted);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task Update_RejectsInvalidName_WithoutPersisting()
    {
        var goal = CreateGoal();
        var repository = new FakeGoalRepository(goal);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new UpdateGoalHandler(repository).HandleAsync(
                new UpdateGoalCommand(goal.Id.Value, "   ", "Changed")));

        Assert.Equal("Goal", goal.Name);
        Assert.Equal("Description", goal.Description);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task Update_RejectsInvalidDescription_WithoutPartialMutation()
    {
        var goal = CreateGoal(isCompleted: true);
        var repository = new FakeGoalRepository(goal);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new UpdateGoalHandler(repository).HandleAsync(
                new UpdateGoalCommand(
                    goal.Id.Value,
                    "Changed name",
                    new string(
                        'x',
                        DomainTextLimits.DescriptionMaximumLength + 1))));

        Assert.Equal("Goal", goal.Name);
        Assert.Equal("Description", goal.Description);
        Assert.True(goal.IsCompleted);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task Update_ThrowsWhenGoalDoesNotExist()
    {
        await Assert.ThrowsAsync<GoalNotFoundException>(() =>
            new UpdateGoalHandler(new FakeGoalRepository()).HandleAsync(
                new UpdateGoalCommand(Guid.NewGuid(), "Name", null)));
    }

    [Fact]
    public async Task Delete_RemovesExistingGoal()
    {
        var goal = CreateGoal();
        var repository = new FakeGoalRepository(goal);

        await new DeleteGoalHandler(repository).HandleAsync(goal.Id.Value);

        Assert.Null(await repository.GetByIdAsync(goal.Id));
    }

    [Fact]
    public async Task Delete_ThrowsWhenGoalDoesNotExist()
    {
        await Assert.ThrowsAsync<GoalNotFoundException>(() =>
            new DeleteGoalHandler(new FakeGoalRepository()).HandleAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CompleteAndReopen_KeepExistingGoalData()
    {
        var goal = CreateGoal();
        var repository = new FakeGoalRepository(goal);

        await new CompleteGoalHandler(repository).HandleAsync(goal.Id.Value);
        Assert.True(goal.IsCompleted);
        Assert.Equal("Goal", goal.Name);
        Assert.Equal("Description", goal.Description);

        await new ReopenGoalHandler(repository).HandleAsync(goal.Id.Value);
        Assert.False(goal.IsCompleted);
        Assert.Equal("Goal", goal.Name);
        Assert.Equal("Description", goal.Description);
    }

    private static Goal CreateGoal(bool isCompleted = false) =>
        new(GoalId.New(), AreaId.New(), "Goal", "Description", isCompleted);

    private sealed class FakeGoalRepository(params Goal[] goals) : IGoalRepository
    {
        private readonly Dictionary<GoalId, Goal> _goals =
            goals.ToDictionary(goal => goal.Id);

        public int UpdateCount { get; private set; }

        public Task AddAsync(Goal goal, CancellationToken cancellationToken = default)
        {
            _goals.Add(goal.Id, goal);
            return Task.CompletedTask;
        }

        public Task<Goal?> GetByIdAsync(GoalId goalId, CancellationToken cancellationToken = default)
        {
            _goals.TryGetValue(goalId, out var goal);
            return Task.FromResult(goal);
        }

        public Task UpdateAsync(Goal goal, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            _goals[goal.Id] = goal;
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(GoalId goalId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_goals.Remove(goalId));
    }
}
