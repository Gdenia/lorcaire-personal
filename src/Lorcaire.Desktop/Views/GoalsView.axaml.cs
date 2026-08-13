using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Goals.CreateGoal;
using Lorcaire.Application.Goals.GetGoals;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Views;

public partial class GoalsView : UserControl
{
    private readonly CreateGoalHandler _createGoalHandler;
    private readonly GetGoalsHandler _getGoalsHandler;
    private readonly WorkspaceContext _workspaceContext;

    // Constructor requerido por el cargador XAML de Avalonia.
    public GoalsView()
        : this(
            App.Services.GetRequiredService<CreateGoalHandler>(),
            App.Services.GetRequiredService<GetGoalsHandler>(),
            App.Services.GetRequiredService<WorkspaceContext>())
    {
    }

    // Constructor principal con dependencias explícitas.
    public GoalsView(
        CreateGoalHandler createGoalHandler,
        GetGoalsHandler getGoalsHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createGoalHandler);
        ArgumentNullException.ThrowIfNull(getGoalsHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createGoalHandler = createGoalHandler;
        _getGoalsHandler = getGoalsHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();

        Loaded += LoadGoals;
    }

    private async void LoadGoals(
        object? sender,
        RoutedEventArgs e)
    {
        try
        {
            await RefreshGoalsAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to load goals: {exception.Message}";
        }
    }

    private async void CreateGoal(
        object? sender,
        RoutedEventArgs e)
    {
        CreateGoalButton.IsEnabled = false;
        OperationMessage.Text = string.Empty;

        try
        {
            var command = new CreateGoalCommand(
                _workspaceContext.DefaultAreaId,
                GoalName.Text ?? string.Empty,
                GoalDescription.Text);

            await _createGoalHandler.HandleAsync(command);

            GoalName.Text = string.Empty;
            GoalDescription.Text = string.Empty;

            await RefreshGoalsAsync();

            OperationMessage.Text =
                "Goal created successfully.";
        }
        catch (ArgumentException exception)
        {
            OperationMessage.Text = exception.Message;
        }
        catch (AreaNotFoundException exception)
        {
            OperationMessage.Text = exception.Message;
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to create the goal: {exception.Message}";
        }
        finally
        {
            CreateGoalButton.IsEnabled = true;
        }
    }

    private async Task RefreshGoalsAsync()
    {
        var goals = await _getGoalsHandler.HandleAsync();

        GoalsList.ItemsSource = goals;

        if (goals.Count == 0)
        {
            OperationMessage.Text =
                "You have not created any goals yet.";
        }
    }
}
