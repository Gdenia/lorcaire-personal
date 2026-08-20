using Lorcaire.Application.Resources.GetResources;
using Lorcaire.Application.Resources.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Resources;

namespace Lorcaire.Application.Tests.Resources;

public sealed class GetResourcesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsResourceSummaries()
    {
        var resource = new Resource(
            ResourceId.New(),
            AreaId.New(),
            "Clean Architecture",
            "Book",
            "Reference");
        var handler = new GetResourcesHandler(
            new FakeResourceReader([resource]));

        var summary = Assert.Single(await handler.HandleAsync());

        Assert.Equal(resource.Id.Value, summary.Id);
        Assert.Equal(resource.AreaId.Value, summary.AreaId);
        Assert.Equal(resource.Name, summary.Name);
        Assert.Equal(resource.Category, summary.Category);
        Assert.Equal(resource.Description, summary.Description);
    }

    private sealed class FakeResourceReader(
        IReadOnlyList<Resource> resources) : IResourceReader
    {
        public Task<IReadOnlyList<Resource>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(resources);
    }
}
