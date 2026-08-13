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
}
