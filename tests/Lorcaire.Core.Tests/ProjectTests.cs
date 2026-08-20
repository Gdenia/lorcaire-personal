using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Core.Tests.Domain.Projects;

public sealed class ProjectTests
{
    [Fact]
    public void Constructor_CreatesProject_WithNormalizedData()
    {
        var id = ProjectId.New();
        var areaId = AreaId.New();
        var project = new Project(
            id,
            areaId,
            "  Lanzar producto  ",
            "  Primera versión.  ");

        Assert.Equal(id, project.Id);
        Assert.Equal(areaId, project.AreaId);
        Assert.Equal("Lanzar producto", project.Name);
        Assert.Equal("Primera versión.", project.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyName(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Project(ProjectId.New(), AreaId.New(), name));
    }

    [Fact]
    public void Rename_ChangesName()
    {
        var project = CreateProject();
        project.Rename("Nuevo nombre");
        Assert.Equal("Nuevo nombre", project.Name);
    }

    [Fact]
    public void ChangeDescription_NormalizesEmptyDescriptionToNull()
    {
        var project = CreateProject();
        project.ChangeDescription("   ");
        Assert.Null(project.Description);
    }

    [Fact]
    public void ProjectId_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new ProjectId(Guid.Empty));
    }

    private static Project CreateProject() =>
        new(ProjectId.New(), AreaId.New(), "Proyecto");
}
