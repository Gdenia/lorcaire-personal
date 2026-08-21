using Lorcaire.Application.Errors;
using Lorcaire.Application.Projects;
using Lorcaire.Application.Projects.DeleteProject;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Projects.UpdateProject;
using Lorcaire.Application.Tasks.Persistence;
using Lorcaire.Core.Domain;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;
using TaskId = Lorcaire.Core.Domain.Tasks.TaskId;

namespace Lorcaire.Application.Tests.Projects;

public sealed class ManageProjectHandlersTests
{
    [Fact]
    public async Task UpdateAndDelete_WorkAndMissingIsReported()
    {
        var project = new Project(ProjectId.New(), AreaId.New(), "Old");
        var repository = new Repo(project);
        var taskReader = new TaskReader([]);
        await new UpdateProjectHandler(repository).HandleAsync(
            new(project.Id.Value, "New", "Desc"));
        Assert.Equal("New", project.Name);
        await new DeleteProjectHandler(repository, taskReader)
            .HandleAsync(project.Id.Value);
        await Assert.ThrowsAsync<ProjectNotFoundException>(() =>
            new DeleteProjectHandler(repository, taskReader)
                .HandleAsync(project.Id.Value));
    }

    [Fact]
    public async Task Delete_RejectsProjectWithAssignedTasks()
    {
        var project = new Project(ProjectId.New(), AreaId.New(), "Project");
        var repository = new Repo(project);
        var task = new DomainTask(
            TaskId.New(), project.AreaId, "Task", projectId: project.Id);

        await Assert.ThrowsAsync<ConflictException>(() =>
            new DeleteProjectHandler(repository, new TaskReader([task]))
                .HandleAsync(project.Id.Value));

        Assert.NotNull(await repository.GetByIdAsync(project.Id));
    }

    [Fact]
    public async Task UpdateFailure_DoesNotPartiallyMutateProject()
    {
        var project = new Project(
            ProjectId.New(), AreaId.New(), "Old", "Original");
        var repository = new Repo(project);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new UpdateProjectHandler(repository).HandleAsync(new(
                project.Id.Value,
                "New",
                new string(
                    'x',
                    DomainTextLimits.DescriptionMaximumLength + 1))));
        Assert.Equal("Old", project.Name);
        Assert.Equal("Original", project.Description);
    }

    private sealed class Repo(params Project[] values) : IProjectRepository
    {
        private readonly Dictionary<ProjectId, Project> _items =
            values.ToDictionary(project => project.Id);

        public Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            _items.Add(project.Id, project);
            return Task.CompletedTask;
        }

        public Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default)
        {
            _items.TryGetValue(id, out var project);
            return Task.FromResult(project);
        }

        public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            _items[project.Id] = project;
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(ProjectId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Remove(id));
    }

    private sealed class TaskReader(IReadOnlyList<DomainTask> tasks) : ITaskReader
    {
        public Task<IReadOnlyList<DomainTask>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(tasks);
    }
}
