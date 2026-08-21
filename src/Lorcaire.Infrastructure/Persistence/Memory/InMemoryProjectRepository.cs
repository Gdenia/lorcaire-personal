using System.Collections.Concurrent;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Errors;
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
            throw new ConflictException(
                "A project with the same identifier already exists.");
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
    public Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); _projects.TryGetValue(id, out var value); return Task.FromResult(value); }
    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    { ArgumentNullException.ThrowIfNull(project); cancellationToken.ThrowIfCancellationRequested(); if (!_projects.ContainsKey(project.Id)) throw new ConflictException("The project could not be updated because it no longer exists."); _projects[project.Id] = project; return Task.CompletedTask; }
    public Task<bool> DeleteAsync(ProjectId id, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); return Task.FromResult(_projects.TryRemove(id, out _)); }
}
