using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Application.Projects.Persistence;

public interface IProjectRepository
{
    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);
}
