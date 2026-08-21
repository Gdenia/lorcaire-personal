using Lorcaire.Application.Errors;

namespace Lorcaire.Application.Goals;

public sealed class GoalNotFoundException : NotFoundException
{
    public GoalNotFoundException(Guid goalId)
        : base($"No goal exists with identifier '{goalId}'.")
    {
    }
}
