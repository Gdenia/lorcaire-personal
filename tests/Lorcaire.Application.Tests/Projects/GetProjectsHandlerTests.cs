using Lorcaire.Application.Projects.GetProjects;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Application.Tests.Projects;

public sealed class GetProjectsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsProjectSummaries()
    {
        var project = new Project(
            ProjectId.New(),
            AreaId.New(),
            "Proyecto",
            "Descripción");
        var handler = new GetProjectsHandler(new FakeProjectReader([project]));

        var result = await handler.HandleAsync();
        var summary = Assert.Single(result);

        Assert.Equal(project.Id.Value, summary.Id);
        Assert.Equal(project.AreaId.Value, summary.AreaId);
        Assert.Equal(project.Name, summary.Name);
        Assert.Equal(project.Description, summary.Description);
    }

    private sealed class FakeProjectReader(
        IReadOnlyList<Project> projects) : IProjectReader
    {
        public Task<IReadOnlyList<Project>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(projects);
    }
}
