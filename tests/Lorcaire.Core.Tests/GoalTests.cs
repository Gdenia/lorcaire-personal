using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Core.Tests.Domain.Goals;

public sealed class GoalTests
{
    [Fact]
    public void Constructor_CreatesGoal_WithValidData()
    {
        var goalId = GoalId.New();
        var areaId = AreaId.New();

        var goal = new Goal(
            goalId,
            areaId,
            "Aprender arquitectura",
            "Comprender y aplicar la arquitectura de Lorcaire.");

        Assert.Equal(goalId, goal.Id);
        Assert.Equal(areaId, goal.AreaId);
        Assert.Equal("Aprender arquitectura", goal.Name);
        Assert.Equal(
            "Comprender y aplicar la arquitectura de Lorcaire.",
            goal.Description);
    }

    [Fact]
    public void Constructor_TrimsNameAndDescription()
    {
        var goal = new Goal(
            GoalId.New(),
            AreaId.New(),
            "  Mejorar mi salud  ",
            "  Mantener hábitos saludables.  ");

        Assert.Equal("Mejorar mi salud", goal.Name);
        Assert.Equal("Mantener hábitos saludables.", goal.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyName(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Goal(
                GoalId.New(),
                AreaId.New(),
                name));
    }

    [Fact]
    public void Rename_ChangesName()
    {
        var goal = CreateGoal();

        goal.Rename("Nuevo resultado deseado");

        Assert.Equal("Nuevo resultado deseado", goal.Name);
    }

    [Fact]
    public void Rename_RejectsEmptyName_AndPreservesCurrentName()
    {
        var goal = CreateGoal();
        var originalName = goal.Name;

        Assert.Throws<ArgumentException>(() => goal.Rename("   "));

        Assert.Equal(originalName, goal.Name);
    }

    [Fact]
    public void ChangeDescription_NormalizesEmptyDescriptionToNull()
    {
        var goal = CreateGoal();

        goal.ChangeDescription("   ");

        Assert.Null(goal.Description);
    }

    [Fact]
    public void GoalId_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() =>
            new GoalId(Guid.Empty));
    }

    [Fact]
    public void AreaId_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() =>
            new AreaId(Guid.Empty));
    }

    private static Goal CreateGoal()
    {
        return new Goal(
            GoalId.New(),
            AreaId.New(),
            "Resultado deseado",
            "Descripción del resultado.");
    }
}
