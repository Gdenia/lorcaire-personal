using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.CreateTask;
using Lorcaire.Application.Tasks.GetTasks;
using Lorcaire.Application.Tasks.UpdateTask;
using Lorcaire.Application.Tasks.DeleteTask;
using Lorcaire.Application.Settings;

namespace Lorcaire.Desktop.Views;

public partial class TasksView : UserControl
{
    private readonly CreateTaskHandler _createTaskHandler;
    private readonly GetTasksHandler _getTasksHandler;
    private readonly CompleteTaskHandler _completeTaskHandler;
    private readonly ReopenTaskHandler _reopenTaskHandler;
    private readonly GetUserPreferencesHandler _getPreferencesHandler;
    private readonly UpdateTaskHandler _updateTaskHandler;
    private readonly DeleteTaskHandler _deleteTaskHandler;
    private readonly WorkspaceContext _workspaceContext;
    private IReadOnlyList<TaskSummary> _tasks=[];
    private Guid? _editingId;
    private Guid? _pendingDeleteId;

    public TasksView(
        CreateTaskHandler createTaskHandler,
        GetTasksHandler getTasksHandler,
        CompleteTaskHandler completeTaskHandler,
        ReopenTaskHandler reopenTaskHandler,
        GetUserPreferencesHandler getPreferencesHandler,
        UpdateTaskHandler updateTaskHandler,
        DeleteTaskHandler deleteTaskHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createTaskHandler);
        ArgumentNullException.ThrowIfNull(getTasksHandler);
        ArgumentNullException.ThrowIfNull(completeTaskHandler);
        ArgumentNullException.ThrowIfNull(reopenTaskHandler);
        ArgumentNullException.ThrowIfNull(getPreferencesHandler);
        ArgumentNullException.ThrowIfNull(updateTaskHandler);
        ArgumentNullException.ThrowIfNull(deleteTaskHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createTaskHandler = createTaskHandler;
        _getTasksHandler = getTasksHandler;
        _completeTaskHandler = completeTaskHandler;
        _reopenTaskHandler = reopenTaskHandler;
        _getPreferencesHandler = getPreferencesHandler;
        _updateTaskHandler = updateTaskHandler;
        _deleteTaskHandler = deleteTaskHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        Loaded += LoadTasks;
    }
    private void BeginEdit(object? sender,RoutedEventArgs e){if(sender is not Button{Tag:Guid id})return;var item=_tasks.Single(x=>x.Id==id);_editingId=id;TaskTitle.Text=item.Title;TaskDescription.Text=item.Description;FormTitle.Text="Edit task";CreateTaskButton.IsVisible=false;SaveTaskButton.IsVisible=true;CancelTaskButton.IsVisible=true;}
    private async void SaveTask(object? sender,RoutedEventArgs e){if(_editingId is not Guid id)return;try{await _updateTaskHandler.HandleAsync(new(id,TaskTitle.Text??string.Empty,TaskDescription.Text));ResetForm();await RefreshTasksAsync();OperationMessage.Text="Task updated.";}catch(Exception ex){OperationMessage.Text=$"Unable to update task: {ex.Message}";}}
    private void CancelEdit(object? sender,RoutedEventArgs e)=>ResetForm();
    private void DeleteTask(object? sender,RoutedEventArgs e){if(sender is not Button{Tag:Guid id})return;_pendingDeleteId=id;ConfirmDeleteButton.IsVisible=true;CancelDeleteButton.IsVisible=true;OperationMessage.Text="Confirm or cancel the deletion.";}
    private async void ConfirmDelete(object? sender,RoutedEventArgs e){if(_pendingDeleteId is not Guid id)return;try{await _deleteTaskHandler.HandleAsync(id);if(_editingId==id)ResetForm();await RefreshTasksAsync();OperationMessage.Text="Task deleted.";}catch(Exception ex){OperationMessage.Text=$"Unable to delete task: {ex.Message}";}finally{ClearDelete();}}
    private void CancelDelete(object? sender,RoutedEventArgs e){ClearDelete();OperationMessage.Text="Deletion cancelled.";}
    private void ClearDelete(){_pendingDeleteId=null;ConfirmDeleteButton.IsVisible=false;CancelDeleteButton.IsVisible=false;}
    private void ResetForm(){_editingId=null;TaskTitle.Text="";TaskDescription.Text="";FormTitle.Text="Create a task";CreateTaskButton.IsVisible=true;SaveTaskButton.IsVisible=false;CancelTaskButton.IsVisible=false;}

    private async void LoadTasks(object? sender, RoutedEventArgs e)
    {
        try
        {
            await RefreshTasksAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to load tasks: {exception.Message}";
        }
    }

    private async void CreateTask(object? sender, RoutedEventArgs e)
    {
        CreateTaskButton.IsEnabled = false;
        OperationMessage.Text = string.Empty;

        try
        {
            await _createTaskHandler.HandleAsync(
                new CreateTaskCommand(
                    _workspaceContext.DefaultAreaId,
                    TaskTitle.Text ?? string.Empty,
                    TaskDescription.Text));

            TaskTitle.Text = string.Empty;
            TaskDescription.Text = string.Empty;
            OperationMessage.Text = "Task created.";
            await RefreshTasksAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to create task: {exception.Message}";
        }
        finally
        {
            CreateTaskButton.IsEnabled = true;
        }
    }

    private async void CompleteTask(object? sender, RoutedEventArgs e)
    {
        await ChangeStatusAsync(sender, complete: true);
    }

    private async void ReopenTask(object? sender, RoutedEventArgs e)
    {
        await ChangeStatusAsync(sender, complete: false);
    }

    private async System.Threading.Tasks.Task ChangeStatusAsync(
        object? sender,
        bool complete)
    {
        if (sender is not Button { Tag: Guid taskId } button)
        {
            return;
        }

        button.IsEnabled = false;
        OperationMessage.Text = string.Empty;

        try
        {
            if (complete)
            {
                await _completeTaskHandler.HandleAsync(taskId);
                OperationMessage.Text = "Task completed.";
            }
            else
            {
                await _reopenTaskHandler.HandleAsync(taskId);
                OperationMessage.Text = "Task reopened.";
            }

            await RefreshTasksAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to update task: {exception.Message}";
        }
        finally
        {
            button.IsEnabled = true;
        }
    }

    private async System.Threading.Tasks.Task RefreshTasksAsync()
    {
        _tasks = await _getTasksHandler.HandleAsync();
        var preferences = await _getPreferencesHandler.HandleAsync();

        TasksList.ItemsSource = preferences.ShowCompletedTasks
            ? _tasks
            : _tasks.Where(task => !task.IsCompleted).ToArray();
    }
}
