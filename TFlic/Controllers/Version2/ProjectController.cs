using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFlic.Controllers.Version2.DTO.GET;
using TFlic.Controllers.Version2.DTO.POST;
using TFlic.Controllers.Version2.Service;
using TFlic.Models.Contexts;
using TFlic.Models.Organization.Project;

namespace TFlic.Controllers.Version2;

#if AUTH
using Models.Authentication;
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("api/v2")]
public class ProjectController : ControllerBase
{
    public ProjectController(ProjectContext projectContext, OrganizationContext organizationContext)
    {
        _projectContext = projectContext;
        _organizationContext = organizationContext;
    }

    #region GET
    [HttpGet("organizations/{organizationId}/projects")]
    public ActionResult<IEnumerable<ProjectGet>> GetProjects(ulong organizationId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        var organization = _organizationContext.Organizations
            .SingleOrDefault(org => org.Id == organizationId);
        if (organization is null)
            return NotFound();

        var projects = _projectContext.Projects
            .Where(project => project.OrganizationId == organizationId)
            .Select(project => new ProjectGet(project));

        return Ok(projects);
    }
    
    [HttpGet("projects/{projectId}")]
    public ActionResult<ProjectGet> GetProject(ulong projectId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif

        var project = _projectContext.Projects.SingleOrDefault(project => project.id == projectId);
        return project is not null
            ? new ProjectGet(project)
            : NotFound();
    }
    
    #endregion

    #region DELETE
    [HttpDelete("projects/{projectId}")]
    public ActionResult DeleteProjects(ulong projectId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif

        var projectToDelete = _projectContext.Projects.SingleOrDefault(project => project.id == projectId);
        if (projectToDelete is null)
            return NotFound();

        _projectContext.Projects.Remove(projectToDelete);
        _projectContext.SaveChanges();

        return Ok();
    }
    #endregion

    #region POST
    [HttpPost("organizations/{organizationId}/projects")]
    public ActionResult<ProjectGet> CreateProject(ulong organizationId, ProjectDto project)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif

        var organization = _organizationContext.Organizations
            .SingleOrDefault(org => org.Id == organizationId);
        if (organization is null)
            return NotFound();

        var newProject = new Project
        {
            name = project.Name,
            OrganizationId = organizationId
        };
        _projectContext.Projects.Add(newProject);
        _projectContext.SaveChanges();
            
        return Ok(new ProjectGet(newProject));
    }
    #endregion

    #region PATCH
    [HttpPatch("projects/{projectId}")]
    public ActionResult<ProjectGet> PatchProject(ulong projectId, [FromBody] JsonPatchDocument<Project> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif

        var projectToPatch = _projectContext.Projects.SingleOrDefault(project => project.id == projectId);
        if (projectToPatch is null)
            return NotFound();
        
        patch.ApplyTo(projectToPatch);
        _projectContext.SaveChanges();
        
        return Ok(new ProjectGet(projectToPatch));
    }
    #endregion
    
    
    
    private readonly ProjectContext _projectContext;
    private readonly OrganizationContext _organizationContext;
}
