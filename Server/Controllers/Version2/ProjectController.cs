using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Controllers.Version2.DTO.GET;
using Server.Controllers.Version2.DTO.POST;
using Server.Controllers.Version2.Service;
using Server.Models.Contexts;
using Server.Models.Organization.Project;

namespace Server.Controllers.Version2;

#if AUTH
using Models.Authentication;
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("api/v2/organizations/{organizationId}")]
public class ProjectController : ControllerBase
{
    private readonly ILogger<ProjectController> _logger;

    public ProjectController(ILogger<ProjectController> logger)
    {
        _logger = logger;
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
        using var ctx = DbContexts.Get<ProjectContext>();
        var cmp = ContextIncluder.GetProject(ctx)
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
        using var ctx = DbContexts.Get<ProjectContext>();
        var cmp = ContextIncluder.GetProject(ctx)
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
        using var ctx = DbContexts.Get<ProjectContext>();
        var cmp = ContextIncluder.DeleteProject(ctx).Where(x => x.id == projectId && x.OrganizationId == organizationId);
        ctx.Projects.RemoveRange(cmp);
        ctx.SaveChanges();
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
        
        using var ctx = DbContexts.Get<ProjectContext>();
        var obj = new Project
        {
            name = project.Name,
            OrganizationId = organizationId
        };
        ctx.Projects.Add(obj);
        ctx.SaveChanges();
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
        using var ctx = DbContexts.Get<ProjectContext>();
        var obj = ContextIncluder.GetProject(ctx)
            .Include(x => x.boards)
            .Where(x => x.id == projectId && x.OrganizationId == organizationId)
            .ToList();
        patch.ApplyTo(obj.Single());
        ctx.SaveChanges();
        
        return Ok(new ProjectGet(obj.Single()));
    }
    #endregion
}
