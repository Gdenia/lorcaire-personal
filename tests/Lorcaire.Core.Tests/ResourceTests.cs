using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Resources;

namespace Lorcaire.Core.Tests.Domain.Resources;

public sealed class ResourceTests
{
    [Fact]
    public void Constructor_CreatesResource_WithNormalizedData()
    {
        var id = ResourceId.New();
        var areaId = AreaId.New();
        var resource = new Resource(
            id,
            areaId,
            "  Clean Architecture  ",
            "  Book  ",
            "  Reference material.  ");

        Assert.Equal(id, resource.Id);
        Assert.Equal(areaId, resource.AreaId);
        Assert.Equal("Clean Architecture", resource.Name);
        Assert.Equal("Book", resource.Category);
        Assert.Equal("Reference material.", resource.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyName(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Resource(
                ResourceId.New(),
                AreaId.New(),
                name,
                "Book"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyCategory(string category)
    {
        Assert.Throws<ArgumentException>(() =>
            new Resource(
                ResourceId.New(),
                AreaId.New(),
                "Resource",
                category));
    }

    [Fact]
    public void Rename_ChangesName()
    {
        var resource = CreateResource();
        resource.Rename("New name");
        Assert.Equal("New name", resource.Name);
    }

    [Fact]
    public void ChangeCategory_ChangesCategory()
    {
        var resource = CreateResource();
        resource.ChangeCategory("Course");
        Assert.Equal("Course", resource.Category);
    }

    [Fact]
    public void ChangeDescription_NormalizesEmptyDescriptionToNull()
    {
        var resource = CreateResource();
        resource.ChangeDescription(" ");
        Assert.Null(resource.Description);
    }

    [Fact]
    public void ResourceId_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new ResourceId(Guid.Empty));
    }

    private static Resource CreateResource() =>
        new(ResourceId.New(), AreaId.New(), "Resource", "Book", "Description");
}
