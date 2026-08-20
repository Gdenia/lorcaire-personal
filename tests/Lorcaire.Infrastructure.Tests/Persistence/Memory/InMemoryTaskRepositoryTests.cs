using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Tasks;
using Lorcaire.Infrastructure.Persistence.Memory;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;

namespace Lorcaire.Infrastructure.Tests.Persistence.Memory;

public sealed class InMemoryTaskRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task Repository_PersistsAndOrdersTasks()
    {
        var repository = new InMemoryTaskRepository();
        var areaId = AreaId.New();
        var completed = new DomainTask(TaskId.New(), areaId, "Completada");
        completed.Complete();
        await repository.AddAsync(completed);
        await repository.AddAsync(
            new DomainTask(TaskId.New(), areaId, "Pendiente"));

        var tasks = await repository.GetAllAsync();

        Assert.Collection(
            tasks,
            task => Assert.Equal("Pendiente", task.Title),
            task => Assert.Equal("Completada", task.Title));
    }

    [Fact]
    public async System.Threading.Tasks.Task Repository_UpdatesTask()
    {
        var repository = new InMemoryTaskRepository();
        var task = new DomainTask(TaskId.New(), AreaId.New(), "Tarea");
        await repository.AddAsync(task);
        task.Complete();
        await repository.UpdateAsync(task);

        Assert.True((await repository.GetByIdAsync(task.Id))!.IsCompleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task Repository_RejectsDuplicateId()
    {
        var repository = new InMemoryTaskRepository();
        var task = new DomainTask(TaskId.New(), AreaId.New(), "Tarea");
        await repository.AddAsync(task);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(task));
    }
}
