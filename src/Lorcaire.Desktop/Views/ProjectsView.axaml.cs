using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Projects.CreateProject;
using Lorcaire.Application.Projects.GetProjects;
using Lorcaire.Application.Projects.DeleteProject;
using Lorcaire.Application.Projects.UpdateProject;
using Lorcaire.Desktop.Presentation;
using Lorcaire.Core.Domain;

namespace Lorcaire.Desktop.Views;

public partial class ProjectsView : UserControl
{
    private readonly CreateProjectHandler _createProjectHandler;
    private readonly GetProjectsHandler _getProjectsHandler;
    private readonly UpdateProjectHandler _updateProjectHandler;
    private readonly DeleteProjectHandler _deleteProjectHandler;
    private readonly WorkspaceContext _workspaceContext;
    private IReadOnlyList<ProjectSummary> _projects = [];
    private Guid? _editingId;
    private Guid? _pendingDeleteId;

    public ProjectsView(
        CreateProjectHandler createProjectHandler,
        GetProjectsHandler getProjectsHandler,
        UpdateProjectHandler updateProjectHandler,
        DeleteProjectHandler deleteProjectHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createProjectHandler);
        ArgumentNullException.ThrowIfNull(getProjectsHandler);
        ArgumentNullException.ThrowIfNull(updateProjectHandler);
        ArgumentNullException.ThrowIfNull(deleteProjectHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createProjectHandler = createProjectHandler;
        _getProjectsHandler = getProjectsHandler;
        _updateProjectHandler = updateProjectHandler;
        _deleteProjectHandler = deleteProjectHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        ProjectName.MaxLength = DomainTextLimits.NameMaximumLength;
        ProjectDescription.MaxLength = DomainTextLimits.DescriptionMaximumLength;
        Loaded += LoadProjects;
    }

    private void BeginEdit(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Guid id }) return; var item = _projects.Single(x => x.Id == id); _editingId=id; ProjectName.Text=item.Name; ProjectDescription.Text=item.Description; FormTitle.Text="Edit project"; CreateProjectButton.IsVisible=false; SaveProjectButton.IsVisible=true; CancelProjectButton.IsVisible=true;
    }
    private async void SaveProject(object? sender, RoutedEventArgs e)
    {
        if (_editingId is not Guid id) return; try { await _updateProjectHandler.HandleAsync(new UpdateProjectCommand(id, ProjectName.Text ?? string.Empty, ProjectDescription.Text)); ResetForm(); await RefreshProjectsAsync(); OperationMessage.Text="Project updated."; } catch(Exception ex) { OperationMessage.Text=UserErrorMessages.Format("Unable to update project",ex); }
    }
    private void CancelEdit(object? sender, RoutedEventArgs e) => ResetForm();
    private void DeleteProject(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Guid id }) return; _pendingDeleteId=id; ConfirmDeleteButton.IsVisible=true; CancelDeleteButton.IsVisible=true; OperationMessage.Text="Confirm or cancel the deletion.";
    }
    private async void ConfirmDelete(object? sender, RoutedEventArgs e)
    {
        if (_pendingDeleteId is not Guid id) return;
        try { await _deleteProjectHandler.HandleAsync(id); if (_editingId==id) ResetForm(); await RefreshProjectsAsync(); OperationMessage.Text="Project deleted."; } catch(Exception ex) { OperationMessage.Text=UserErrorMessages.Format("Unable to delete project",ex); } finally { ClearDelete(); }
    }
    private void CancelDelete(object? sender, RoutedEventArgs e) { ClearDelete(); OperationMessage.Text="Deletion cancelled."; }
    private void ClearDelete() { _pendingDeleteId=null; ConfirmDeleteButton.IsVisible=false; CancelDeleteButton.IsVisible=false; }
    private void ResetForm() { _editingId=null; ProjectName.Text=""; ProjectDescription.Text=""; FormTitle.Text="Create a project"; CreateProjectButton.IsVisible=true; SaveProjectButton.IsVisible=false; CancelProjectButton.IsVisible=false; }

    private async void LoadProjects(object? sender, RoutedEventArgs e)
    {
        try
        {
            await RefreshProjectsAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text = UserErrorMessages.Format(
                "Unable to load projects",
                exception);
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
            OperationMessage.Text = UserErrorMessages.Format(
                "Unable to create project",
                exception);
        }
        finally
        {
            CreateProjectButton.IsEnabled = true;
        }
    }

    private async Task RefreshProjectsAsync()
    {
        _projects = await _getProjectsHandler.HandleAsync();
        ProjectsList.ItemsSource = _projects;
    }
}
