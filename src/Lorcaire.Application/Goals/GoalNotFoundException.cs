namespace Lorcaire.Application.Goals;

public sealed class GoalNotFoundException : Exception
{
    public GoalNotFoundException(Guid goalId)
        : base($"No goal exists with identifier '{goalId}'.")
    {
    }
}
