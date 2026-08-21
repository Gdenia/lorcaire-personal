using Lorcaire.Application.Projects;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.DeleteTask;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Application.Tasks.UpdateTask;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using Lorcaire.Core.Domain.Tasks;
using Lorcaire.Core.Domain;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
namespace Lorcaire.Application.Tests.Tasks;
public sealed class ManageTaskHandlersTests
{
    [Fact]
    public async System.Threading.Tasks.Task UpdatePreservesStatus_AndDeleteReportsMissing()
    {
        var task = new DomainTask(TaskId.New(), AreaId.New(), "Old", isCompleted: true);
        var repository = new Repository(task);
        await new UpdateTaskHandler(repository, new ProjectRepository()).HandleAsync(new(task.Id.Value, "New", "Desc"));
        Assert.True(task.IsCompleted);
        Assert.Equal("New", task.Title);
        await new DeleteTaskHandler(repository).HandleAsync(task.Id.Value);
        await Assert.ThrowsAsync<TaskNotFoundException>(() => new DeleteTaskHandler(repository).HandleAsync(task.Id.Value));
    }
    [Fact]
    public async System.Threading.Tasks.Task UpdateFailure_DoesNotPartiallyMutateTask()
    {
        var task = new DomainTask(TaskId.New(), AreaId.New(), "Old", "Original", true);
        var repository = new Repository(task);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new UpdateTaskHandler(repository, new ProjectRepository()).HandleAsync(new(
                task.Id.Value,
                "New",
                new string('x', DomainTextLimits.DescriptionMaximumLength + 1))));
        Assert.Equal("Old", task.Title);
        Assert.Equal("Original", task.Description);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task Update_ChangesAndRemovesProject_WithoutChangingStatus()
    {
        var first = new Project(ProjectId.New(), AreaId.New(), "First");
        var second = new Project(ProjectId.New(), first.AreaId, "Second");
        var task = new DomainTask(
            TaskId.New(),
            first.AreaId,
            "Task",
            isCompleted: true,
            projectId: first.Id);
        var repository = new Repository(task);
        var projects = new ProjectRepository(first, second);
        var handler = new UpdateTaskHandler(repository, projects);

        await handler.HandleAsync(new(
            task.Id.Value,
            "Changed",
            null,
            second.Id.Value));

        Assert.Equal(second.Id, task.ProjectId);
        Assert.True(task.IsCompleted);

        await handler.HandleAsync(new(
            task.Id.Value,
            "Changed again",
            null,
            ProjectId: null));

        Assert.Null(task.ProjectId);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task Update_RejectsUnknownProject_WithoutChangingTask()
    {
        var project = new Project(ProjectId.New(), AreaId.New(), "Current");
        var task = new DomainTask(
            TaskId.New(),
            project.AreaId,
            "Old",
            "Original",
            projectId: project.Id);
        var handler = new UpdateTaskHandler(
            new Repository(task),
            new ProjectRepository(project));

        await Assert.ThrowsAsync<ProjectNotFoundException>(() =>
            handler.HandleAsync(new(
                task.Id.Value,
                "New",
                "Changed",
                Guid.NewGuid())));

        Assert.Equal("Old", task.Title);
        Assert.Equal("Original", task.Description);
        Assert.Equal(project.Id, task.ProjectId);
    }
    private sealed class Repository(params DomainTask[] values) : ITaskRepository
    {
        private readonly Dictionary<TaskId, DomainTask> _items = values.ToDictionary(x => x.Id);
        public System.Threading.Tasks.Task AddAsync(DomainTask task, CancellationToken c = default) { _items.Add(task.Id, task); return System.Threading.Tasks.Task.CompletedTask; }
        public System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(TaskId id, CancellationToken c = default) { _items.TryGetValue(id, out var task); return System.Threading.Tasks.Task.FromResult(task); }
        public System.Threading.Tasks.Task UpdateAsync(DomainTask task, CancellationToken c = default) { _items[task.Id] = task; return System.Threading.Tasks.Task.CompletedTask; }
        public System.Threading.Tasks.Task<bool> DeleteAsync(TaskId id, CancellationToken c = default) => System.Threading.Tasks.Task.FromResult(_items.Remove(id));
    }

    private sealed class ProjectRepository(
        params Project[] projects) : IProjectRepository
    {
        private readonly Dictionary<ProjectId, Project> _items =
            projects.ToDictionary(project => project.Id);

        public System.Threading.Tasks.Task<Project?> GetByIdAsync(
            ProjectId id,
            CancellationToken cancellationToken = default)
        {
            _items.TryGetValue(id, out var project);
            return System.Threading.Tasks.Task.FromResult(project);
        }

        public System.Threading.Tasks.Task AddAsync(Project project, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public System.Threading.Tasks.Task UpdateAsync(Project project, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public System.Threading.Tasks.Task<bool> DeleteAsync(ProjectId id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
