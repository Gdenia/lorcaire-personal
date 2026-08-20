using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Core.Tests.Domain.Notes;

public sealed class NoteTests
{
    [Fact]
    public void Constructor_CreatesNote_WithNormalizedData()
    {
        var now = DateTimeOffset.UtcNow;
        var note = new Note(
            NoteId.New(),
            AreaId.New(),
            "  Architecture  ",
            "  Important content.  ",
            now);

        Assert.Equal("Architecture", note.Title);
        Assert.Equal("Important content.", note.Content);
        Assert.Equal(now, note.CreatedAt);
        Assert.Equal(now, note.LastModifiedAt);
    }

    [Theory]
    [InlineData("", "Content")]
    [InlineData("Title", "")]
    [InlineData(" ", "Content")]
    [InlineData("Title", " ")]
    public void Constructor_RejectsEmptyRequiredData(
        string title,
        string content)
    {
        Assert.Throws<ArgumentException>(() =>
            new Note(
                NoteId.New(),
                AreaId.New(),
                title,
                content,
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_RejectsModificationBeforeCreation()
    {
        var createdAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            new Note(
                NoteId.New(),
                AreaId.New(),
                "Title",
                "Content",
                createdAt,
                createdAt.AddMinutes(-1)));
    }

    [Fact]
    public void Update_ChangesContentAndModificationDate()
    {
        var note = CreateNote();
        var modifiedAt = note.CreatedAt.AddHours(1);

        note.Update("New title", "New content", modifiedAt);

        Assert.Equal("New title", note.Title);
        Assert.Equal("New content", note.Content);
        Assert.Equal(modifiedAt, note.LastModifiedAt);
    }

    [Fact]
    public void Update_RejectsEarlierModification_AndPreservesData()
    {
        var note = CreateNote();

        Assert.Throws<ArgumentException>(() =>
            note.Update(
                "New title",
                "New content",
                note.LastModifiedAt.AddMinutes(-1)));

        Assert.Equal("Title", note.Title);
        Assert.Equal("Content", note.Content);
    }

    [Fact]
    public void Update_RejectsEmptyContent_AndPreservesData()
    {
        var note = CreateNote();

        Assert.Throws<ArgumentException>(() =>
            note.Update("New title", " ", note.LastModifiedAt.AddMinutes(1)));

        Assert.Equal("Title", note.Title);
        Assert.Equal("Content", note.Content);
    }

    [Fact]
    public void NoteId_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new NoteId(Guid.Empty));
    }

    private static Note CreateNote() =>
        new(
            NoteId.New(),
            AreaId.New(),
            "Title",
            "Content",
            DateTimeOffset.UtcNow);
}
