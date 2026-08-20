using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Tasks.ChangeTaskStatus;
using Lorcaire.Application.Tasks.CreateTask;
using Lorcaire.Application.Tasks.GetTasks;
using Lorcaire.Application.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Views;

public partial class TasksView : UserControl
{
    private readonly CreateTaskHandler _createTaskHandler;
    private readonly GetTasksHandler _getTasksHandler;
    private readonly CompleteTaskHandler _completeTaskHandler;
    private readonly ReopenTaskHandler _reopenTaskHandler;
    private readonly GetUserPreferencesHandler _getPreferencesHandler;
    private readonly WorkspaceContext _workspaceContext;

    public TasksView()
        : this(
            App.Services.GetRequiredService<CreateTaskHandler>(),
            App.Services.GetRequiredService<GetTasksHandler>(),
            App.Services.GetRequiredService<CompleteTaskHandler>(),
            App.Services.GetRequiredService<ReopenTaskHandler>(),
            App.Services.GetRequiredService<GetUserPreferencesHandler>(),
            App.Services.GetRequiredService<WorkspaceContext>())
    {
    }

    public TasksView(
        CreateTaskHandler createTaskHandler,
        GetTasksHandler getTasksHandler,
        CompleteTaskHandler completeTaskHandler,
        ReopenTaskHandler reopenTaskHandler,
        GetUserPreferencesHandler getPreferencesHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createTaskHandler);
        ArgumentNullException.ThrowIfNull(getTasksHandler);
        ArgumentNullException.ThrowIfNull(completeTaskHandler);
        ArgumentNullException.ThrowIfNull(reopenTaskHandler);
        ArgumentNullException.ThrowIfNull(getPreferencesHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createTaskHandler = createTaskHandler;
        _getTasksHandler = getTasksHandler;
        _completeTaskHandler = completeTaskHandler;
        _reopenTaskHandler = reopenTaskHandler;
        _getPreferencesHandler = getPreferencesHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        Loaded += LoadTasks;
    }

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
        var tasks = await _getTasksHandler.HandleAsync();
        var preferences = await _getPreferencesHandler.HandleAsync();

        TasksList.ItemsSource = preferences.ShowCompletedTasks
            ? tasks
            : tasks.Where(task => !task.IsCompleted).ToArray();
    }
}
