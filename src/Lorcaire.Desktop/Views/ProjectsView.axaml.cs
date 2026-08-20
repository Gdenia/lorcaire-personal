using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Projects.CreateProject;
using Lorcaire.Application.Projects.GetProjects;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Views;

public partial class ProjectsView : UserControl
{
    private readonly CreateProjectHandler _createProjectHandler;
    private readonly GetProjectsHandler _getProjectsHandler;
    private readonly WorkspaceContext _workspaceContext;

    public ProjectsView()
        : this(
            App.Services.GetRequiredService<CreateProjectHandler>(),
            App.Services.GetRequiredService<GetProjectsHandler>(),
            App.Services.GetRequiredService<WorkspaceContext>())
    {
    }

    public ProjectsView(
        CreateProjectHandler createProjectHandler,
        GetProjectsHandler getProjectsHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createProjectHandler);
        ArgumentNullException.ThrowIfNull(getProjectsHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createProjectHandler = createProjectHandler;
        _getProjectsHandler = getProjectsHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        Loaded += LoadProjects;
    }

    private async void LoadProjects(object? sender, RoutedEventArgs e)
    {
        try
        {
            await RefreshProjectsAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to load projects: {exception.Message}";
        }
    }

    private async void CreateProject(object? sender, RoutedEventArgs e)
    {
        CreateProjectButton.IsEnabled = false;
        OperationMessage.Text = string.Empty;

        try
        {
            var command = new CreateProjectCommand(
                _workspaceContext.DefaultAreaId,
                ProjectName.Text ?? string.Empty,
                ProjectDescription.Text);

            await _createProjectHandler.HandleAsync(command);
            ProjectName.Text = string.Empty;
            ProjectDescription.Text = string.Empty;
            OperationMessage.Text = "Project created.";
            await RefreshProjectsAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to create project: {exception.Message}";
        }
        finally
        {
            CreateProjectButton.IsEnabled = true;
        }
    }

    private async Task RefreshProjectsAsync()
    {
        ProjectsList.ItemsSource =
            await _getProjectsHandler.HandleAsync();
    }
}
