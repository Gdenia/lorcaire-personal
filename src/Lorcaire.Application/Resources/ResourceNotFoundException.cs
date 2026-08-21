using Lorcaire.Application.Errors;

namespace Lorcaire.Application.Resources;
public sealed class ResourceNotFoundException(Guid id):NotFoundException($"No resource exists with identifier '{id}'.");
