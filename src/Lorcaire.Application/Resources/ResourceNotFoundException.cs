namespace Lorcaire.Application.Resources;
public sealed class ResourceNotFoundException(Guid id):Exception($"No resource exists with identifier '{id}'.");
