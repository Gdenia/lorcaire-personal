using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;

namespace Lorcaire.Core.Domain.Tasks;

public sealed class Task
{
    public TaskId Id { get; }
    public AreaId AreaId { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public ProjectId? ProjectId { get; private set; }

    public Task(
        TaskId id,
        AreaId areaId,
        string title,
        string? description = null,
        bool isCompleted = false,
        ProjectId? projectId = null)
    {
        DomainValidation.EnsureIdentifier(id.Value, "task", nameof(id));
        DomainValidation.EnsureIdentifier(areaId.Value, "area", nameof(areaId));
        Id = id;
        AreaId = areaId;
        Title = ValidateTitle(title);
        Description = NormalizeDescription(description);
        IsCompleted = isCompleted;
        ProjectId = ValidateProjectId(projectId);
    }

    public void Rename(string title) => Title = ValidateTitle(title);

    public void ChangeDescription(string? description) =>
        Description = NormalizeDescription(description);

    public void UpdateDetails(string title, string? description)
    {
        var validatedTitle = ValidateTitle(title);
        var validatedDescription = NormalizeDescription(description);

        Title = validatedTitle;
        Description = validatedDescription;
    }

    public void UpdateDetails(
        string title,
        string? description,
        ProjectId? projectId)
    {
        var validatedTitle = ValidateTitle(title);
        var validatedDescription = NormalizeDescription(description);
        var validatedProjectId = ValidateProjectId(projectId);

        Title = validatedTitle;
        Description = validatedDescription;
        ProjectId = validatedProjectId;
    }

    public void AssignToProject(ProjectId projectId) =>
        ProjectId = ValidateProjectId(projectId);

    public void RemoveFromProject() => ProjectId = null;

    public void Complete() => IsCompleted = true;

    public void Reopen() => IsCompleted = false;

    private static string ValidateTitle(string title)
    {
        return DomainValidation.RequiredText(
            title,
            DomainTextLimits.TitleMaximumLength,
            "task title",
            nameof(title));
    }

    private static string? NormalizeDescription(string? description) =>
        DomainValidation.OptionalText(
            description,
            DomainTextLimits.DescriptionMaximumLength,
            "task description",
            nameof(description));

    private static ProjectId? ValidateProjectId(ProjectId? projectId)
    {
        if (projectId is { } value)
        {
            DomainValidation.EnsureIdentifier(
                value.Value,
                "project",
                nameof(projectId));
        }

        return projectId;
    }
}
