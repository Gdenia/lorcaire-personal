using Lorcaire.Application.Resources.Persistence;using Lorcaire.Core.Domain.Resources;
namespace Lorcaire.Application.Resources.DeleteResource;
public sealed class DeleteResourceHandler(IResourceRepository repository){public async Task HandleAsync(Guid id,CancellationToken c=default){if(!await repository.DeleteAsync(new ResourceId(id),c))throw new ResourceNotFoundException(id);}}
