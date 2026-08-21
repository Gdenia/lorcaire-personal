using Lorcaire.Application.Projects;
using Lorcaire.Application.Projects.DeleteProject;
using Lorcaire.Application.Projects.Persistence;
using Lorcaire.Application.Projects.UpdateProject;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using Lorcaire.Core.Domain;
namespace Lorcaire.Application.Tests.Projects;
public sealed class ManageProjectHandlersTests
{
 [Fact] public async Task UpdateAndDelete_WorkAndMissingIsReported()
 { var p=new Project(ProjectId.New(),AreaId.New(),"Old"); var r=new Repo(p); await new UpdateProjectHandler(r).HandleAsync(new(p.Id.Value,"New","Desc")); Assert.Equal("New",p.Name); await new DeleteProjectHandler(r).HandleAsync(p.Id.Value); await Assert.ThrowsAsync<ProjectNotFoundException>(()=>new DeleteProjectHandler(r).HandleAsync(p.Id.Value)); }
 [Fact] public async Task UpdateFailure_DoesNotPartiallyMutateProject()
 { var p=new Project(ProjectId.New(),AreaId.New(),"Old","Original");var r=new Repo(p);await Assert.ThrowsAsync<ArgumentException>(()=>new UpdateProjectHandler(r).HandleAsync(new(p.Id.Value,"New",new string('x',DomainTextLimits.DescriptionMaximumLength+1))));Assert.Equal("Old",p.Name);Assert.Equal("Original",p.Description); }
 private sealed class Repo(params Project[] values):IProjectRepository { private readonly Dictionary<ProjectId,Project> d=values.ToDictionary(x=>x.Id); public Task AddAsync(Project p,CancellationToken c=default){d.Add(p.Id,p);return Task.CompletedTask;} public Task<Project?> GetByIdAsync(ProjectId id,CancellationToken c=default){d.TryGetValue(id,out var p);return Task.FromResult(p);} public Task UpdateAsync(Project p,CancellationToken c=default){d[p.Id]=p;return Task.CompletedTask;} public Task<bool> DeleteAsync(ProjectId id,CancellationToken c=default)=>Task.FromResult(d.Remove(id)); }
}
