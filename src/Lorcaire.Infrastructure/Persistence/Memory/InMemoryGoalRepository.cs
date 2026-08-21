using System.Collections.Concurrent;
using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Errors;
using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Infrastructure.Persistence.Memory;

public sealed class InMemoryGoalRepository :
    IGoalRepository,
    IGoalReader
{
    private readonly ConcurrentDictionary<GoalId, Goal> _goals = [];

    public Task AddAsync(
        Goal goal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(goal);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_goals.TryAdd(goal.Id, goal))
        {
            throw new ConflictException(
                "A goal with the same identifier already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Goal>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Goal> goals = _goals.Values
            .OrderBy(goal => goal.Name)
            .ToArray();

        return Task.FromResult(goals);
    }

    public Task<Goal?> GetByIdAsync(
        GoalId goalId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _goals.TryGetValue(goalId, out var goal);
        return Task.FromResult(goal);
    }

    public Task UpdateAsync(
        Goal goal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(goal);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_goals.TryGetValue(goal.Id, out _))
        {
            throw new ConflictException(
                "The goal could not be updated because it no longer exists.");
        }

        _goals[goal.Id] = goal;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(
        GoalId goalId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_goals.TryRemove(goalId, out _));
    }
}
