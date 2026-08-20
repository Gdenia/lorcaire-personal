namespace Lorcaire.Application.Projects;
public sealed class ProjectNotFoundException(Guid id) : Exception($"No project exists with identifier '{id}'.");
