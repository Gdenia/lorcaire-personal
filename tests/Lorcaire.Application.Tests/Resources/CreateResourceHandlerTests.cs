using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Resources.CreateResource;
using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Resources;

namespace Lorcaire.Application.Tests.Resources;

public sealed class CreateResourceHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesAndStoresResource()
    {
        var repository = new FakeResourceRepository();
        var areaId = Guid.NewGuid();
        var handler = new CreateResourceHandler(
            new FakeAreaRepository(true),
            repository);

        var result = await handler.HandleAsync(
            new CreateResourceCommand(
                areaId,
                "Clean Architecture",
                "Book",
                "Reference"));

        var resource = Assert.Single(repository.Resources);
        Assert.Equal(result.ResourceId, resource.Id.Value);
        Assert.Equal(areaId, resource.AreaId.Value);
        Assert.Equal("Clean Architecture", resource.Name);
        Assert.Equal("Book", resource.Category);
    }

    [Fact]
    public async Task HandleAsync_RejectsUnknownArea()
    {
        var repository = new FakeResourceRepository();
        var handler = new CreateResourceHandler(
            new FakeAreaRepository(false),
            repository);

        await Assert.ThrowsAsync<AreaNotFoundException>(() =>
            handler.HandleAsync(
                new CreateResourceCommand(
                    Guid.NewGuid(),
                    "Resource",
                    "Book",
                    null)));
        Assert.Empty(repository.Resources);
    }

    [Theory]
    [InlineData("", "Book")]
    [InlineData("Resource", "")]
    public async Task HandleAsync_RejectsInvalidRequiredData(
        string name,
        string category)
    {
        var repository = new FakeResourceRepository();
        var handler = new CreateResourceHandler(
            new FakeAreaRepository(true),
            repository);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(
                new CreateResourceCommand(
                    Guid.NewGuid(),
                    name,
                    category,
                    null)));
        Assert.Empty(repository.Resources);
    }

    private sealed class FakeAreaRepository(bool exists) : IAreaRepository
    {
        public Task<bool> ExistsAsync(
            AreaId areaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);
    }

    private sealed class FakeResourceRepository : IResourceRepository
    {
        public List<Resource> Resources { get; } = [];

        public Task AddAsync(
            Resource resource,
            CancellationToken cancellationToken = default)
        {
            Resources.Add(resource);
            return Task.CompletedTask;
        }
        public Task<Resource?> GetByIdAsync(ResourceId id,CancellationToken c=default)=>Task.FromResult(Resources.SingleOrDefault(x=>x.Id==id));
        public Task UpdateAsync(Resource resource,CancellationToken c=default)=>Task.CompletedTask;
        public Task<bool> DeleteAsync(ResourceId id,CancellationToken c=default)=>Task.FromResult(Resources.RemoveAll(x=>x.Id==id)==1);
    }
}
