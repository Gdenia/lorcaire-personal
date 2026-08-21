using Lorcaire.Application.Resources.Persistence;using Lorcaire.Core.Domain.Resources;
namespace Lorcaire.Application.Resources.UpdateResource;
public sealed class UpdateResourceHandler(IResourceRepository repository){public async Task HandleAsync(UpdateResourceCommand command,CancellationToken c=default){ArgumentNullException.ThrowIfNull(command);var item=await repository.GetByIdAsync(new ResourceId(command.ResourceId),c)??throw new ResourceNotFoundException(command.ResourceId);item.UpdateDetails(command.Name,command.Category,command.Description);await repository.UpdateAsync(item,c);}}
