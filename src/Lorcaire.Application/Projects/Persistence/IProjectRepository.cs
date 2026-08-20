using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Application.Projects.Persistence;

public interface IProjectRepository
{
    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(ProjectId projectId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(ProjectId projectId, CancellationToken cancellationToken = default);
}
