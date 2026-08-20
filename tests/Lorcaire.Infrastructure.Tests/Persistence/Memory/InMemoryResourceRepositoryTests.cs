using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Resources;
using Lorcaire.Infrastructure.Persistence.Memory;

namespace Lorcaire.Infrastructure.Tests.Persistence.Memory;

public sealed class InMemoryResourceRepositoryTests
{
    [Fact]
    public async Task Repository_PersistsAndOrdersResources()
    {
        var repository = new InMemoryResourceRepository();
        var areaId = AreaId.New();
        await repository.AddAsync(
            new Resource(ResourceId.New(), areaId, "Visual Studio", "Tool"));
        await repository.AddAsync(
            new Resource(ResourceId.New(), areaId, "Architecture", "Book"));

        var resources = await repository.GetAllAsync();

        Assert.Collection(
            resources,
            resource => Assert.Equal("Architecture", resource.Name),
            resource => Assert.Equal("Visual Studio", resource.Name));
    }

    [Fact]
    public async Task Repository_RejectsDuplicateId()
    {
        var repository = new InMemoryResourceRepository();
        var resource = new Resource(
            ResourceId.New(),
            AreaId.New(),
            "Resource",
            "Book");
        await repository.AddAsync(resource);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(resource));
    }
}
