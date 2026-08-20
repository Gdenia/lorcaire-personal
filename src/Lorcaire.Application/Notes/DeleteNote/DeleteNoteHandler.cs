using Lorcaire.Application.Notes.Persistence;using Lorcaire.Application.Notes.UpdateNote;using Lorcaire.Core.Domain.Notes;
namespace Lorcaire.Application.Notes.DeleteNote;
public sealed class DeleteNoteHandler(INoteRepository repository){public async Task HandleAsync(Guid id,CancellationToken c=default){if(!await repository.DeleteAsync(new NoteId(id),c))throw new NoteNotFoundException(id);}}
