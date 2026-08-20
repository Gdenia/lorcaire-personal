using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Projects.CreateProject;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Application.Tests.Projects;

public sealed class CreateProjectHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesAndStoresProject_WhenAreaExists()
    {
        var repository = new FakeProjectRepository();
        var areaId = Guid.NewGuid();
        var handler = new CreateProjectHandler(
            new FakeAreaRepository(true),
            repository);

        var result = await handler.HandleAsync(
            new CreateProjectCommand(areaId, "Proyecto", "Descripción"));

        var project = Assert.Single(repository.Projects);
        Assert.Equal(result.ProjectId, project.Id.Value);
        Assert.Equal(areaId, project.AreaId.Value);
        Assert.Equal("Proyecto", project.Name);
    }

    [Fact]
    public async Task HandleAsync_RejectsRequest_WhenAreaDoesNotExist()
    {
        var repository = new FakeProjectRepository();
        var handler = new CreateProjectHandler(
            new FakeAreaRepository(false),
            repository);

        await Assert.ThrowsAsync<AreaNotFoundException>(() =>
            handler.HandleAsync(
                new CreateProjectCommand(Guid.NewGuid(), "Proyecto", null)));

        Assert.Empty(repository.Projects);
    }

    [Fact]
    public async Task HandleAsync_DoesNotStoreProject_WhenNameIsInvalid()
    {
        var repository = new FakeProjectRepository();
        var handler = new CreateProjectHandler(
            new FakeAreaRepository(true),
            repository);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(
                new CreateProjectCommand(Guid.NewGuid(), "   ", null)));

        Assert.Empty(repository.Projects);
    }

    private sealed class FakeAreaRepository(bool exists) : IAreaRepository
    {
        public Task<bool> ExistsAsync(
            AreaId areaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        public List<Project> Projects { get; } = [];

        public Task AddAsync(
            Project project,
            CancellationToken cancellationToken = default)
        {
            Projects.Add(project);
            return Task.CompletedTask;
        }
        public Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default) => Task.FromResult(Projects.SingleOrDefault(x => x.Id == id));
        public Task UpdateAsync(Project project, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> DeleteAsync(ProjectId id, CancellationToken cancellationToken = default) => Task.FromResult(Projects.RemoveAll(x => x.Id == id) == 1);
    }
}
