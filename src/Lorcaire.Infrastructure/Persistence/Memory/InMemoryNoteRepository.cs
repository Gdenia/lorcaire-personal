using System.Collections.Concurrent;
using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Application.Errors;
using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Infrastructure.Persistence.Memory;

public sealed class InMemoryNoteRepository : INoteRepository, INoteReader
{
    private readonly ConcurrentDictionary<NoteId, Note> _notes = [];

    public Task AddAsync(
        Note note,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_notes.TryAdd(note.Id, note))
        {
            throw new ConflictException(
                "A note with the same identifier already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<Note?> GetByIdAsync(
        NoteId noteId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _notes.TryGetValue(noteId, out var note);
        return Task.FromResult(note);
    }

    public Task UpdateAsync(
        Note note,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_notes.ContainsKey(note.Id))
        {
            throw new ConflictException(
                "The note could not be updated because it no longer exists.");
        }

        _notes[note.Id] = note;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Note>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Note> notes = _notes.Values
            .OrderByDescending(note => note.LastModifiedAt)
            .ThenBy(note => note.Title)
            .ToArray();
        return Task.FromResult(notes);
    }
    public Task<bool> DeleteAsync(NoteId id,CancellationToken c=default){c.ThrowIfCancellationRequested();return Task.FromResult(_notes.TryRemove(id,out _));}
}
