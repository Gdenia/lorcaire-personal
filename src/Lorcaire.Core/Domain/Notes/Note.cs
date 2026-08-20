using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Core.Domain.Notes;

public sealed class Note
{
    public NoteId Id { get; }
    public AreaId AreaId { get; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset LastModifiedAt { get; private set; }

    public Note(
        NoteId id,
        AreaId areaId,
        string title,
        string content,
        DateTimeOffset createdAt,
        DateTimeOffset? lastModifiedAt = null)
    {
        var effectiveLastModifiedAt = lastModifiedAt ?? createdAt;
        ValidateTimeline(createdAt, effectiveLastModifiedAt);

        Id = id;
        AreaId = areaId;
        Title = ValidateRequired(
            title,
            "La nota debe tener un título.",
            nameof(title));
        Content = ValidateRequired(
            content,
            "La nota debe tener contenido.",
            nameof(content));
        CreatedAt = createdAt;
        LastModifiedAt = effectiveLastModifiedAt;
    }

    public void Update(
        string title,
        string content,
        DateTimeOffset modifiedAt)
    {
        ValidateTimeline(LastModifiedAt, modifiedAt);
        var validatedTitle = ValidateRequired(
            title,
            "La nota debe tener un título.",
            nameof(title));
        var validatedContent = ValidateRequired(
            content,
            "La nota debe tener contenido.",
            nameof(content));

        Title = validatedTitle;
        Content = validatedContent;
        LastModifiedAt = modifiedAt;
    }

    private static string ValidateRequired(
        string value,
        string message,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }

        return value.Trim();
    }

    private static void ValidateTimeline(
        DateTimeOffset earlier,
        DateTimeOffset later)
    {
        if (later < earlier)
        {
            throw new ArgumentException(
                "La fecha de modificación no puede ser anterior.",
                nameof(later));
        }
    }
}
