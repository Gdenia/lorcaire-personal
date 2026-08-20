using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Resources.CreateResource;
using Lorcaire.Application.Resources.GetResources;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Views;

public partial class ResourcesView : UserControl
{
    private readonly CreateResourceHandler _createResourceHandler;
    private readonly GetResourcesHandler _getResourcesHandler;
    private readonly WorkspaceContext _workspaceContext;

    public ResourcesView()
        : this(
            App.Services.GetRequiredService<CreateResourceHandler>(),
            App.Services.GetRequiredService<GetResourcesHandler>(),
            App.Services.GetRequiredService<WorkspaceContext>())
    {
    }

    public ResourcesView(
        CreateResourceHandler createResourceHandler,
        GetResourcesHandler getResourcesHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createResourceHandler);
        ArgumentNullException.ThrowIfNull(getResourcesHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createResourceHandler = createResourceHandler;
        _getResourcesHandler = getResourcesHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        Loaded += LoadResources;
    }

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
        ResourcesList.ItemsSource =
            await _getResourcesHandler.HandleAsync();
    }
}
