namespace Lorcaire.Application.Notes.GetNotes;

public sealed record NoteSummary(
    Guid Id,
    Guid AreaId,
    string Title,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastModifiedAt)
{
    public string LastModifiedDisplay =>
        $"Modified {LastModifiedAt.ToLocalTime():g}";
}
