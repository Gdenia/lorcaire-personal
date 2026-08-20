using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Application.Projects.Persistence;

public interface IProjectReader
{
    Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
