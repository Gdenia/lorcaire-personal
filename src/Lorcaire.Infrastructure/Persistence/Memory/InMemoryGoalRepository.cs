using System.Collections.Concurrent;
using Lorcaire.Application.Goals.Persistence;
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
            throw new InvalidOperationException(
                $"Ya existe un objetivo con identificador '{goal.Id}'.");
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
            throw new InvalidOperationException(
                $"No existe un objetivo con identificador '{goal.Id}'.");
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
