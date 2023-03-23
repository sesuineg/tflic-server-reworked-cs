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
[Route("api/v2/organizations/{organizationId}")]
public class ProjectController : ControllerBase
{
    public ProjectController(ProjectContext projectContext)
    {
        _projectContext = projectContext;
    }

    #region GET
    
    [HttpGet("projects")]
    public ActionResult<ICollection<ProjectGet>> GetProjects(ulong organizationId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(organizationId))
                    return NotFound();

        var cmp = ContextIncluder.GetProject(_projectContext)
            .Include(x => x.boards)
            .Where(x => x.OrganizationId == organizationId)
            .Select(x => new ProjectGet(x))
            .ToList();
        return Ok(cmp);
    }
    
    [HttpGet("projects/{projectId}")]
    public ActionResult<ProjectGet> GetProject(ulong organizationId, ulong projectId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(organizationId))
            return NotFound();

        var cmp = ContextIncluder.GetProject(_projectContext)
            .Where(x => x.OrganizationId == organizationId && x.id == projectId)
            .Include(x => x.boards)
            .Select(x => new ProjectGet(x))
            .ToList();
        return !cmp.Any() ? NotFound() : Ok(cmp.Single());
    }
    
    #endregion

    #region DELETE
    [HttpDelete("projects/{projectId}")]
    public ActionResult DeleteProjects(ulong organizationId, ulong projectId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(organizationId))
            return NotFound();

        var cmp = ContextIncluder.DeleteProject(_projectContext).Where(x => x.id == projectId && x.OrganizationId == organizationId);
        _projectContext.Projects.RemoveRange(cmp);
        _projectContext.SaveChanges();
        return Ok();
    }
    #endregion

    #region POST
    [HttpPost("projects")]
    public ActionResult<ProjectGet> PostProjects(ulong organizationId, ProjectDto project)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(organizationId))
            return NotFound();
        
        var obj = new Project
        {
            name = project.Name,
            OrganizationId = organizationId
        };
        _projectContext.Projects.Add(obj);
        _projectContext.SaveChanges();
        return Ok(new ProjectGet(obj));
    }
    #endregion

    #region PATCH
    [HttpPatch("projects/{projectId}")]
    public ActionResult<ProjectGet> PatchProject(ulong organizationId, ulong projectId, [FromBody] JsonPatchDocument<Project> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(organizationId))
            return NotFound();

        var obj = ContextIncluder.GetProject(_projectContext)
            .Include(x => x.boards)
            .Where(x => x.id == projectId && x.OrganizationId == organizationId)
            .ToList();
        patch.ApplyTo(obj.Single());
        _projectContext.SaveChanges();
        
        return Ok(new ProjectGet(obj.Single()));
    }
    #endregion
    
    
    
    private readonly ProjectContext _projectContext;
}
