using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Resources.CreateResource;
using Lorcaire.Application.Resources.GetResources;
using Lorcaire.Application.Resources.UpdateResource;
using Lorcaire.Application.Resources.DeleteResource;

namespace Lorcaire.Desktop.Views;

public partial class ResourcesView : UserControl
{
    private readonly CreateResourceHandler _createResourceHandler;
    private readonly GetResourcesHandler _getResourcesHandler;
    private readonly UpdateResourceHandler _updateResourceHandler;
    private readonly DeleteResourceHandler _deleteResourceHandler;
    private readonly WorkspaceContext _workspaceContext;
    private IReadOnlyList<ResourceSummary> _resources=[]; private Guid? _editingId; private Guid? _pendingDeleteId;

    public ResourcesView(
        CreateResourceHandler createResourceHandler,
        GetResourcesHandler getResourcesHandler,
        UpdateResourceHandler updateResourceHandler,
        DeleteResourceHandler deleteResourceHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createResourceHandler);
        ArgumentNullException.ThrowIfNull(getResourcesHandler);
        ArgumentNullException.ThrowIfNull(updateResourceHandler);
        ArgumentNullException.ThrowIfNull(deleteResourceHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createResourceHandler = createResourceHandler;
        _getResourcesHandler = getResourcesHandler;
        _updateResourceHandler = updateResourceHandler;
        _deleteResourceHandler = deleteResourceHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        Loaded += LoadResources;
    }
    private void BeginEdit(object? sender,RoutedEventArgs e){if(sender is not Button{Tag:Guid id})return;var x=_resources.Single(i=>i.Id==id);_editingId=id;ResourceName.Text=x.Name;ResourceCategory.Text=x.Category;ResourceDescription.Text=x.Description;FormTitle.Text="Edit resource";CreateResourceButton.IsVisible=false;SaveResourceButton.IsVisible=true;CancelResourceButton.IsVisible=true;}
    private async void SaveResource(object? sender,RoutedEventArgs e){if(_editingId is not Guid id)return;try{await _updateResourceHandler.HandleAsync(new(id,ResourceName.Text??"",ResourceCategory.Text??"",ResourceDescription.Text));ResetForm();await RefreshResourcesAsync();OperationMessage.Text="Resource updated.";}catch(Exception ex){OperationMessage.Text=$"Unable to update resource: {ex.Message}";}}
    private void CancelEdit(object? sender,RoutedEventArgs e)=>ResetForm();
    private void DeleteResource(object? sender,RoutedEventArgs e){if(sender is not Button{Tag:Guid id})return;_pendingDeleteId=id;ConfirmDeleteButton.IsVisible=true;CancelDeleteButton.IsVisible=true;OperationMessage.Text="Confirm or cancel the deletion.";}
    private async void ConfirmDelete(object? sender,RoutedEventArgs e){if(_pendingDeleteId is not Guid id)return;try{await _deleteResourceHandler.HandleAsync(id);if(_editingId==id)ResetForm();await RefreshResourcesAsync();OperationMessage.Text="Resource deleted.";}catch(Exception ex){OperationMessage.Text=$"Unable to delete resource: {ex.Message}";}finally{ClearDelete();}}
    private void CancelDelete(object? sender,RoutedEventArgs e){ClearDelete();OperationMessage.Text="Deletion cancelled.";}private void ClearDelete(){_pendingDeleteId=null;ConfirmDeleteButton.IsVisible=false;CancelDeleteButton.IsVisible=false;}private void ResetForm(){_editingId=null;ResourceName.Text="";ResourceCategory.Text="";ResourceDescription.Text="";FormTitle.Text="Create a resource";CreateResourceButton.IsVisible=true;SaveResourceButton.IsVisible=false;CancelResourceButton.IsVisible=false;}

    private async void LoadResources(object? sender, RoutedEventArgs e)
    {
        try
        {
            await RefreshResourcesAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to load resources: {exception.Message}";
        }
    }

    private async void CreateResource(object? sender, RoutedEventArgs e)
    {
        CreateResourceButton.IsEnabled = false;
        OperationMessage.Text = string.Empty;

        try
        {
            await _createResourceHandler.HandleAsync(
                new CreateResourceCommand(
                    _workspaceContext.DefaultAreaId,
                    ResourceName.Text ?? string.Empty,
                    ResourceCategory.Text ?? string.Empty,
                    ResourceDescription.Text));

            ResourceName.Text = string.Empty;
            ResourceCategory.Text = string.Empty;
            ResourceDescription.Text = string.Empty;
            OperationMessage.Text = "Resource created.";
            await RefreshResourcesAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to create resource: {exception.Message}";
        }
        finally
        {
            CreateResourceButton.IsEnabled = true;
        }
    }

    private async Task RefreshResourcesAsync()
    {
        _resources=await _getResourcesHandler.HandleAsync(); ResourcesList.ItemsSource=_resources;
    }
}
