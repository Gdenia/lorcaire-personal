using System.Collections.Concurrent;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Infrastructure.Persistence.Memory;

public sealed class InMemoryProjectRepository :
    IProjectRepository,
    IProjectReader
{
    private readonly ConcurrentDictionary<ProjectId, Project> _projects = [];

    public Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_projects.TryAdd(project.Id, project))
        {
            throw new InvalidOperationException(
                $"Ya existe un proyecto con identificador '{project.Id}'.");
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Project> projects = _projects.Values
            .OrderBy(project => project.Name)
            .ToArray();

        return Task.FromResult(projects);
    }
}
