using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using Lorcaire.Infrastructure.Persistence.Memory;

namespace Lorcaire.Infrastructure.Tests.Persistence.Memory;

public sealed class InMemoryProjectRepositoryTests
{
    [Fact]
    public async Task Repository_PersistsAndReadsProjects_OrderedByName()
    {
        var repository = new InMemoryProjectRepository();
        var areaId = AreaId.New();
        await repository.AddAsync(
            new Project(ProjectId.New(), areaId, "Segundo"));
        await repository.AddAsync(
            new Project(ProjectId.New(), areaId, "Primero"));

        var projects = await repository.GetAllAsync();

        Assert.Collection(
            projects,
            project => Assert.Equal("Primero", project.Name),
            project => Assert.Equal("Segundo", project.Name));
    }

    [Fact]
    public async Task Repository_RejectsDuplicatedProjectId()
    {
        var repository = new InMemoryProjectRepository();
        var project = new Project(ProjectId.New(), AreaId.New(), "Proyecto");
        await repository.AddAsync(project);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(project));
    }
}
