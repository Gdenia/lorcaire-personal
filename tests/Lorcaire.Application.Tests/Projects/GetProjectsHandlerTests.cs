using Lorcaire.Application.Projects.GetProjects;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using TaskId = Lorcaire.Core.Domain.Tasks.TaskId;

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
        var handler = new GetProjectsHandler(
            new FakeProjectReader([project]),
            new FakeTaskReader(
            [
                new DomainTask(
                    TaskId.New(),
                    project.AreaId,
                    "Task",
                    projectId: project.Id)
            ]));

        var result = await handler.HandleAsync();
        var summary = Assert.Single(result);

        Assert.Equal(project.Id.Value, summary.Id);
        Assert.Equal(project.AreaId.Value, summary.AreaId);
        Assert.Equal(project.Name, summary.Name);
        Assert.Equal(project.Description, summary.Description);
        Assert.Equal(1, summary.TaskCount);
    }

    private sealed class FakeProjectReader(
        IReadOnlyList<Project> projects) : IProjectReader
    {
        public Task<IReadOnlyList<Project>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(projects);
    }

    private sealed class FakeTaskReader(
        IReadOnlyList<DomainTask> tasks) : ITaskReader
    {
        public System.Threading.Tasks.Task<IReadOnlyList<DomainTask>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.FromResult(tasks);
    }
}
