using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Core.Domain.Tasks;

public sealed class Task
{
    public TaskId Id { get; }
    public AreaId AreaId { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }

    public Task(
        TaskId id,
        AreaId areaId,
        string title,
        string? description = null,
        bool isCompleted = false)
    {
        Id = id;
        AreaId = areaId;
        Title = ValidateTitle(title);
        Description = NormalizeDescription(description);
        IsCompleted = isCompleted;
    }

    public void Rename(string title) => Title = ValidateTitle(title);

    public void ChangeDescription(string? description) =>
        Description = NormalizeDescription(description);

    public void Complete() => IsCompleted = true;

    public void Reopen() => IsCompleted = false;

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "La tarea debe tener un título.",
                nameof(title));
        }

        return title.Trim();
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
