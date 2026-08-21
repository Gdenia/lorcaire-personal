using Lorcaire.Application.Errors;

namespace Lorcaire.Application.Projects;
public sealed class ProjectNotFoundException(Guid id) : NotFoundException($"No project exists with identifier '{id}'.");
