using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Goals.CreateGoal;

public sealed class CreateGoalHandler
{
    private readonly IAreaRepository _areaRepository;
    private readonly IGoalRepository _goalRepository;

    public CreateGoalHandler(
        IAreaRepository areaRepository,
        IGoalRepository goalRepository)
    {
        _areaRepository = areaRepository;
        _goalRepository = goalRepository;
    }

    public async Task<CreateGoalResult> HandleAsync(
        CreateGoalCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var areaId = new AreaId(command.AreaId);

        var areaExists = await _areaRepository.ExistsAsync(
            areaId,
            cancellationToken);

        if (!areaExists)
        {
            throw new AreaNotFoundException(command.AreaId);
        }

        var goal = new Goal(
            GoalId.New(),
            areaId,
            command.Name,
            command.Description);

        await _goalRepository.AddAsync(goal, cancellationToken);

        return new CreateGoalResult(goal.Id.Value);
    }
}
