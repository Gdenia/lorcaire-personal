using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.CreateTask;
using Lorcaire.Application.Tasks.GetTasks;
using Lorcaire.Application.Tasks.UpdateTask;
using Lorcaire.Application.Tasks.DeleteTask;
using Lorcaire.Application.Projects.GetProjects;
using Lorcaire.Application.Settings;
using Lorcaire.Desktop.Presentation;
using Lorcaire.Core.Domain;

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
    private readonly GetProjectsHandler _getProjectsHandler;
    private readonly WorkspaceContext _workspaceContext;
    private IReadOnlyList<TaskSummary> _tasks=[];
    private Guid? _editingId;
    private Guid? _pendingDeleteId;
    private IReadOnlyList<TaskProjectOption> _projectOptions = [];

    public TasksView(
        CreateTaskHandler createTaskHandler,
        GetTasksHandler getTasksHandler,
        CompleteTaskHandler completeTaskHandler,
        ReopenTaskHandler reopenTaskHandler,
        GetUserPreferencesHandler getPreferencesHandler,
        UpdateTaskHandler updateTaskHandler,
        DeleteTaskHandler deleteTaskHandler,
        GetProjectsHandler getProjectsHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createTaskHandler);
        ArgumentNullException.ThrowIfNull(getTasksHandler);
        ArgumentNullException.ThrowIfNull(completeTaskHandler);
        ArgumentNullException.ThrowIfNull(reopenTaskHandler);
        ArgumentNullException.ThrowIfNull(getPreferencesHandler);
        ArgumentNullException.ThrowIfNull(updateTaskHandler);
        ArgumentNullException.ThrowIfNull(deleteTaskHandler);
        ArgumentNullException.ThrowIfNull(getProjectsHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createTaskHandler = createTaskHandler;
        _getTasksHandler = getTasksHandler;
        _completeTaskHandler = completeTaskHandler;
        _reopenTaskHandler = reopenTaskHandler;
        _getPreferencesHandler = getPreferencesHandler;
        _updateTaskHandler = updateTaskHandler;
        _deleteTaskHandler = deleteTaskHandler;
        _getProjectsHandler = getProjectsHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        TaskTitle.MaxLength = DomainTextLimits.TitleMaximumLength;
        TaskDescription.MaxLength = DomainTextLimits.DescriptionMaximumLength;
        Loaded += LoadTasks;
    }
    private void BeginEdit(object? sender,RoutedEventArgs e){if(sender is not Button{Tag:Guid id})return;var item=_tasks.Single(x=>x.Id==id);_editingId=id;TaskTitle.Text=item.Title;TaskDescription.Text=item.Description;SelectProject(item.ProjectId);FormTitle.Text="Edit task";CreateTaskButton.IsVisible=false;SaveTaskButton.IsVisible=true;CancelTaskButton.IsVisible=true;}
    private async void SaveTask(object? sender,RoutedEventArgs e){if(_editingId is not Guid id)return;try{await _updateTaskHandler.HandleAsync(new(id,TaskTitle.Text??string.Empty,TaskDescription.Text,SelectedProjectId));ResetForm();await RefreshTasksAsync();OperationMessage.Text="Task updated.";}catch(Exception ex){OperationMessage.Text=UserErrorMessages.Format("Unable to update task",ex);}}
    private void CancelEdit(object? sender,RoutedEventArgs e)=>ResetForm();
    private void DeleteTask(object? sender,RoutedEventArgs e){if(sender is not Button{Tag:Guid id})return;_pendingDeleteId=id;ConfirmDeleteButton.IsVisible=true;CancelDeleteButton.IsVisible=true;OperationMessage.Text="Confirm or cancel the deletion.";}
    private async void ConfirmDelete(object? sender,RoutedEventArgs e){if(_pendingDeleteId is not Guid id)return;try{await _deleteTaskHandler.HandleAsync(id);if(_editingId==id)ResetForm();await RefreshTasksAsync();OperationMessage.Text="Task deleted.";}catch(Exception ex){OperationMessage.Text=UserErrorMessages.Format("Unable to delete task",ex);}finally{ClearDelete();}}
    private void CancelDelete(object? sender,RoutedEventArgs e){ClearDelete();OperationMessage.Text="Deletion cancelled.";}
    private void ClearDelete(){_pendingDeleteId=null;ConfirmDeleteButton.IsVisible=false;CancelDeleteButton.IsVisible=false;}
    private void ResetForm(){_editingId=null;TaskTitle.Text="";TaskDescription.Text="";SelectProject(null);FormTitle.Text="Create a task";CreateTaskButton.IsVisible=true;SaveTaskButton.IsVisible=false;CancelTaskButton.IsVisible=false;}

    private async void LoadTasks(object? sender, RoutedEventArgs e)
    {
        try
        {
            await RefreshTasksAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text = UserErrorMessages.Format(
                "Unable to load tasks",
                exception);
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
                    TaskDescription.Text,
                    SelectedProjectId));

            TaskTitle.Text = string.Empty;
            TaskDescription.Text = string.Empty;
            OperationMessage.Text = "Task created.";
            await RefreshTasksAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text = UserErrorMessages.Format(
                "Unable to create task",
                exception);
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
            OperationMessage.Text = UserErrorMessages.Format(
                "Unable to update task",
                exception);
        }
        finally
        {
            button.IsEnabled = true;
        }
    }

    private async System.Threading.Tasks.Task RefreshTasksAsync()
    {
        var projects = await _getProjectsHandler.HandleAsync();
        var selectedProjectId = SelectedProjectId;
        _projectOptions =
        [
            new TaskProjectOption(null, "No project"),
            .. projects.Select(project =>
                new TaskProjectOption(project.Id, project.Name))
        ];
        TaskProject.ItemsSource = _projectOptions;
        SelectProject(selectedProjectId);

        _tasks = await _getTasksHandler.HandleAsync();
        var preferences = await _getPreferencesHandler.HandleAsync();

        TasksList.ItemsSource = preferences.ShowCompletedTasks
            ? _tasks
            : _tasks.Where(task => !task.IsCompleted).ToArray();
    }

    private Guid? SelectedProjectId =>
        (TaskProject.SelectedItem as TaskProjectOption)?.Id;

    private void SelectProject(Guid? projectId)
    {
        TaskProject.SelectedItem = _projectOptions
            .FirstOrDefault(option => option.Id == projectId);
    }
}
