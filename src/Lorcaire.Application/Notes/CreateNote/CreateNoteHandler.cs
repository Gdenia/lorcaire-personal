using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Application.Notes.CreateNote;

public sealed class CreateNoteHandler
{
    private readonly IAreaRepository _areaRepository;
    private readonly INoteRepository _noteRepository;
    private readonly TimeProvider _timeProvider;

    public CreateNoteHandler(
        IAreaRepository areaRepository,
        INoteRepository noteRepository,
        TimeProvider timeProvider)
    {
        _areaRepository = areaRepository;
        _noteRepository = noteRepository;
        _timeProvider = timeProvider;
    }

    public async Task<CreateNoteResult> HandleAsync(
        CreateNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var areaId = new AreaId(command.AreaId);

        if (!await _areaRepository.ExistsAsync(areaId, cancellationToken))
        {
            throw new AreaNotFoundException(command.AreaId);
        }

        var note = new Note(
            NoteId.New(),
            areaId,
            command.Title,
            command.Content,
            _timeProvider.GetUtcNow());

        await _noteRepository.AddAsync(note, cancellationToken);
        return new CreateNoteResult(note.Id.Value);
    }
}
