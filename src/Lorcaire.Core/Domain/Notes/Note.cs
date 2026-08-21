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
        DomainValidation.EnsureIdentifier(id.Value, "note", nameof(id));
        DomainValidation.EnsureIdentifier(areaId.Value, "area", nameof(areaId));
        var effectiveLastModifiedAt = lastModifiedAt ?? createdAt;
        ValidateTimeline(createdAt, effectiveLastModifiedAt);

        Id = id;
        AreaId = areaId;
        Title = ValidateRequired(
            title,
            DomainTextLimits.TitleMaximumLength,
            "note title",
            nameof(title));
        Content = ValidateRequired(
            content,
            DomainTextLimits.NoteContentMaximumLength,
            "note content",
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
            DomainTextLimits.TitleMaximumLength,
            "note title",
            nameof(title));
        var validatedContent = ValidateRequired(
            content,
            DomainTextLimits.NoteContentMaximumLength,
            "note content",
            nameof(content));

        Title = validatedTitle;
        Content = validatedContent;
        LastModifiedAt = modifiedAt;
    }

    private static string ValidateRequired(
        string value,
        int maximumLength,
        string fieldName,
        string parameterName)
        => DomainValidation.RequiredText(
            value,
            maximumLength,
            fieldName,
            parameterName);

    private static void ValidateTimeline(
        DateTimeOffset earlier,
        DateTimeOffset later)
    {
        if (later < earlier)
        {
            throw new ArgumentException(
                "The modification time cannot be earlier than the previous time.",
                nameof(later));
        }
    }
}
