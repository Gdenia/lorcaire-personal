using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Goals;
using Lorcaire.Application.Goals.ChangeGoalStatus;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.DeleteGoal;
using Lorcaire.Application.Goals.GetGoals;
using Lorcaire.Application.Goals.UpdateGoal;
using Lorcaire.Desktop.Presentation;
using Lorcaire.Core.Domain;

namespace Lorcaire.Desktop.Views;

public partial class GoalsView : UserControl
{
    private readonly CreateGoalHandler _createGoalHandler;
    private readonly GetGoalsHandler _getGoalsHandler;
    private readonly UpdateGoalHandler _updateGoalHandler;
    private readonly DeleteGoalHandler _deleteGoalHandler;
    private readonly CompleteGoalHandler _completeGoalHandler;
    private readonly ReopenGoalHandler _reopenGoalHandler;
    private readonly WorkspaceContext _workspaceContext;
    private IReadOnlyList<GoalSummary> _goals = [];
    private Guid? _editingGoalId;
    private Guid? _pendingDeleteGoalId;

    public GoalsView(
        CreateGoalHandler createGoalHandler,
        GetGoalsHandler getGoalsHandler,
        UpdateGoalHandler updateGoalHandler,
        DeleteGoalHandler deleteGoalHandler,
        CompleteGoalHandler completeGoalHandler,
        ReopenGoalHandler reopenGoalHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createGoalHandler);
        ArgumentNullException.ThrowIfNull(getGoalsHandler);
        ArgumentNullException.ThrowIfNull(updateGoalHandler);
        ArgumentNullException.ThrowIfNull(deleteGoalHandler);
        ArgumentNullException.ThrowIfNull(completeGoalHandler);
        ArgumentNullException.ThrowIfNull(reopenGoalHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createGoalHandler = createGoalHandler;
        _getGoalsHandler = getGoalsHandler;
        _updateGoalHandler = updateGoalHandler;
        _deleteGoalHandler = deleteGoalHandler;
        _completeGoalHandler = completeGoalHandler;
        _reopenGoalHandler = reopenGoalHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        GoalName.MaxLength = DomainTextLimits.NameMaximumLength;
        GoalDescription.MaxLength = DomainTextLimits.DescriptionMaximumLength;
        Loaded += LoadGoals;
    }

    private async void LoadGoals(object? sender, RoutedEventArgs e)
    {
        try { await RefreshGoalsAsync(); }
        catch (Exception exception) { ShowError("Unable to load goals", exception); }
    }

    private async void CreateGoal(object? sender, RoutedEventArgs e)
    {
        CreateGoalButton.IsEnabled = false;
        OperationMessage.Text = string.Empty;
        try
        {
            await _createGoalHandler.HandleAsync(new CreateGoalCommand(
                _workspaceContext.DefaultAreaId,
                GoalName.Text ?? string.Empty,
                GoalDescription.Text));
            ClearForm();
            await RefreshGoalsAsync();
            OperationMessage.Text = "Goal created successfully.";
        }
        catch (Exception exception) { ShowError("Unable to create the goal", exception); }
        finally { CreateGoalButton.IsEnabled = true; }
    }

    private void BeginEdit(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Guid goalId }) return;
        var goal = _goals.SingleOrDefault(item => item.Id == goalId);
        if (goal is null) return;

        _editingGoalId = goal.Id;
        GoalName.Text = goal.Name;
        GoalDescription.Text = goal.Description;
        FormTitle.Text = "Edit goal";
        CreateGoalButton.IsVisible = false;
        SaveGoalButton.IsVisible = true;
        CancelEditButton.IsVisible = true;
        OperationMessage.Text = "Edit the goal and save your changes.";
    }

    private async void SaveGoal(object? sender, RoutedEventArgs e)
    {
        if (_editingGoalId is not Guid goalId) return;
        SaveGoalButton.IsEnabled = false;
        try
        {
            await _updateGoalHandler.HandleAsync(new UpdateGoalCommand(
                goalId,
                GoalName.Text ?? string.Empty,
                GoalDescription.Text));
            ClearForm();
            await RefreshGoalsAsync();
            OperationMessage.Text = "Goal updated successfully.";
        }
        catch (Exception exception) { ShowError("Unable to update the goal", exception); }
        finally { SaveGoalButton.IsEnabled = true; }
    }

    private void CancelEdit(object? sender, RoutedEventArgs e) => ClearForm();

    private async void CompleteGoal(object? sender, RoutedEventArgs e) =>
        await ChangeStatusAsync(sender, _completeGoalHandler.HandleAsync, "Goal completed.");

    private async void ReopenGoal(object? sender, RoutedEventArgs e) =>
        await ChangeStatusAsync(sender, _reopenGoalHandler.HandleAsync, "Goal reopened.");

    private async Task ChangeStatusAsync(
        object? sender,
        Func<Guid, CancellationToken, Task> changeStatus,
        string successMessage)
    {
        if (sender is not Button { Tag: Guid goalId } button) return;
        button.IsEnabled = false;
        try
        {
            await changeStatus(goalId, default);
            await RefreshGoalsAsync();
            OperationMessage.Text = successMessage;
        }
        catch (Exception exception) { ShowError("Unable to change the goal status", exception); }
        finally { button.IsEnabled = true; }
    }

    private void DeleteGoal(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Guid goalId }) return;
        var goal = _goals.SingleOrDefault(item => item.Id == goalId);
        if (goal is null) return;

        _pendingDeleteGoalId = goalId;
        DeleteConfirmationTitle.Text = $"Delete ‘{goal.Name}’?";
        DeleteConfirmation.IsVisible = true;
        OperationMessage.Text = "Confirm or cancel the deletion below.";
    }

    private async void ConfirmDelete(object? sender, RoutedEventArgs e)
    {
        if (_pendingDeleteGoalId is not Guid goalId) return;

        try
        {
            await _deleteGoalHandler.HandleAsync(goalId);
            if (_editingGoalId == goalId) ClearForm();
            await RefreshGoalsAsync();
            OperationMessage.Text = "Goal deleted successfully.";
        }
        catch (Exception exception) { ShowError("Unable to delete the goal", exception); }
        finally { ClearDeleteConfirmation(); }
    }

    private void CancelDelete(object? sender, RoutedEventArgs e)
    {
        ClearDeleteConfirmation();
        OperationMessage.Text = "Deletion cancelled.";
    }

    private void ClearDeleteConfirmation()
    {
        _pendingDeleteGoalId = null;
        DeleteConfirmation.IsVisible = false;
    }

    private async Task RefreshGoalsAsync()
    {
        _goals = await _getGoalsHandler.HandleAsync();
        GoalsList.ItemsSource = _goals;
        EmptyState.IsVisible = _goals.Count == 0;
    }

    private void ClearForm()
    {
        _editingGoalId = null;
        GoalName.Text = string.Empty;
        GoalDescription.Text = string.Empty;
        FormTitle.Text = "Create a goal";
        CreateGoalButton.IsVisible = true;
        SaveGoalButton.IsVisible = false;
        CancelEditButton.IsVisible = false;
    }

    private void ShowError(string operation, Exception exception)
    {
        OperationMessage.Text = UserErrorMessages.Format(operation, exception);
    }
}
