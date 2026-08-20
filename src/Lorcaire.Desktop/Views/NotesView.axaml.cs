using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Notes.CreateNote;
using Lorcaire.Application.Notes.GetNotes;
using Lorcaire.Application.Notes.UpdateNote;
using Lorcaire.Application.Notes.DeleteNote;

namespace Lorcaire.Desktop.Views;

public partial class NotesView : UserControl
{
    private readonly CreateNoteHandler _createNoteHandler;
    private readonly GetNotesHandler _getNotesHandler;
    private readonly UpdateNoteHandler _updateNoteHandler;
    private readonly DeleteNoteHandler _deleteNoteHandler;
    private readonly WorkspaceContext _workspaceContext;
    private Guid? _selectedNoteId;
    private bool _deletePending;

    public NotesView(
        CreateNoteHandler createNoteHandler,
        GetNotesHandler getNotesHandler,
        UpdateNoteHandler updateNoteHandler,
        DeleteNoteHandler deleteNoteHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createNoteHandler);
        ArgumentNullException.ThrowIfNull(getNotesHandler);
        ArgumentNullException.ThrowIfNull(updateNoteHandler);
        ArgumentNullException.ThrowIfNull(deleteNoteHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createNoteHandler = createNoteHandler;
        _getNotesHandler = getNotesHandler;
        _updateNoteHandler = updateNoteHandler;
        _deleteNoteHandler = deleteNoteHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        Loaded += LoadNotes;
    }

    private async void LoadNotes(object? sender, RoutedEventArgs e)
    {
        try
        {
            await RefreshNotesAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to load notes: {exception.Message}";
        }
    }

    private void NewNote(object? sender, RoutedEventArgs e)
    {
        ResetEditor();
        OperationMessage.Text = string.Empty;
    }

    private void SelectNote(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: NoteSummary note })
        {
            return;
        }

        _selectedNoteId = note.Id;
        NoteTitle.Text = note.Title;
        NoteContent.Text = note.Content;
        EditorHeading.Text = note.Title;
        LastModifiedText.Text = note.LastModifiedDisplay;
        SaveNoteButton.Content = "Save changes";
        DeleteNoteButton.IsVisible = true;
        _deletePending=false;CancelDeleteButton.IsVisible=false;DeleteNoteButton.Content="Delete";
        OperationMessage.Text = string.Empty;
    }
    private async void DeleteNote(object? sender,RoutedEventArgs e){if(_selectedNoteId is not Guid id)return;if(!_deletePending){_deletePending=true;DeleteNoteButton.Content="Confirm delete";CancelDeleteButton.IsVisible=true;OperationMessage.Text="Confirm deletion or cancel.";return;}try{await _deleteNoteHandler.HandleAsync(id);ResetEditor();await RefreshNotesAsync();OperationMessage.Text="Note deleted.";}catch(Exception ex){OperationMessage.Text=$"Unable to delete note: {ex.Message}";}}
    private void CancelDelete(object? sender,RoutedEventArgs e){_deletePending=false;DeleteNoteButton.Content="Delete";CancelDeleteButton.IsVisible=false;OperationMessage.Text="Deletion cancelled.";}

    private async void SaveNote(object? sender, RoutedEventArgs e)
    {
        SaveNoteButton.IsEnabled = false;
        OperationMessage.Text = string.Empty;

        try
        {
            if (_selectedNoteId is Guid noteId)
            {
                await _updateNoteHandler.HandleAsync(
                    new UpdateNoteCommand(
                        noteId,
                        NoteTitle.Text ?? string.Empty,
                        NoteContent.Text ?? string.Empty));
                OperationMessage.Text = "Note updated.";
            }
            else
            {
                await _createNoteHandler.HandleAsync(
                    new CreateNoteCommand(
                        _workspaceContext.DefaultAreaId,
                        NoteTitle.Text ?? string.Empty,
                        NoteContent.Text ?? string.Empty));
                OperationMessage.Text = "Note created.";
            }

            ResetEditor();
            await RefreshNotesAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to save note: {exception.Message}";
        }
        finally
        {
            SaveNoteButton.IsEnabled = true;
        }
    }

    private void ResetEditor()
    {
        _selectedNoteId = null;
        NoteTitle.Text = string.Empty;
        NoteContent.Text = string.Empty;
        EditorHeading.Text = "New note";
        LastModifiedText.Text = string.Empty;
        SaveNoteButton.Content = "Create note";
        DeleteNoteButton.IsVisible = false;
        CancelDeleteButton.IsVisible = false;
        _deletePending = false;
    }

    private async Task RefreshNotesAsync()
    {
        NotesList.ItemsSource = await _getNotesHandler.HandleAsync();
    }
}
