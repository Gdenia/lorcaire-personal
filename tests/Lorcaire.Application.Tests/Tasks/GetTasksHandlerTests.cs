using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Tasks.GetTasks;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using Lorcaire.Core.Domain.Tasks;

namespace Lorcaire.Application.Tests.Tasks;

public sealed class GetTasksHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_ReturnsTaskSummaries()
    {
        var project = new Project(ProjectId.New(), AreaId.New(), "Project");
        var task = new DomainTask(
            TaskId.New(),
            project.AreaId,
            "Tarea",
            "Descripción",
            isCompleted: true,
            projectId: project.Id);
        var handler = new GetTasksHandler(
            new FakeTaskReader([task]),
            new FakeProjectReader([project]));

        var summary = Assert.Single(await handler.HandleAsync());

        Assert.Equal(task.Id.Value, summary.Id);
        Assert.Equal(task.AreaId.Value, summary.AreaId);
        Assert.Equal(task.Title, summary.Title);
        Assert.Equal(task.Description, summary.Description);
        Assert.True(summary.IsCompleted);
        Assert.Equal(project.Id.Value, summary.ProjectId);
        Assert.Equal(project.Name, summary.ProjectName);
    }

    private sealed class FakeTaskReader(
        IReadOnlyList<DomainTask> tasks) : ITaskReader
    {
        public System.Threading.Tasks.Task<IReadOnlyList<DomainTask>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.FromResult(tasks);
    }

    private sealed class FakeProjectReader(
        IReadOnlyList<Project> projects) : IProjectReader
    {
        public System.Threading.Tasks.Task<IReadOnlyList<Project>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            System.Threading.Tasks.Task.FromResult(projects);
    }
}
