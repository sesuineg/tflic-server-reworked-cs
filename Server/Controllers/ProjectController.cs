using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Controllers.DTO.GET;
using Server.Controllers.DTO.POST;
using Server.Controllers.Service;
using Server.Models.Contexts;
using Server.Models.Organization.Project;
using Prj = Server.Models.Organization.Project;
using postDTO = Server.Controllers.DTO.POST.ProjectDTO;

namespace Server.Controllers;

#if AUTH
using Models.Authentication;
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("Organizations/{OrganizationId}")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ProjectController : ControllerBase
{
    private readonly ILogger<ProjectController> _logger;

    public ProjectController(ILogger<ProjectController> logger)
    {
        _logger = logger;
    }

    #region GET
    
    [HttpGet("Projects")]
    public ActionResult<ICollection<ProjectGET>> GetProjects(ulong OrganizationId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(OrganizationId))
                    return NotFound();
        using var ctx = DbContexts.Get<ProjectContext>();
        var cmp = ContextIncluder.GetProject(ctx)
            .Include(x => x.boards)
            .Where(x => x.OrganizationId == OrganizationId)
            .Select(x => new ProjectGET(x))
            .ToList();
        return Ok(cmp);
    }
    
    [HttpGet("Projects/{ProjectId}")]
    public ActionResult<ProjectGET> GetProject(ulong OrganizationId, ulong ProjectId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(OrganizationId))
            return NotFound();
        using var ctx = DbContexts.Get<ProjectContext>();
        var cmp = ContextIncluder.GetProject(ctx)
            .Where(x => x.OrganizationId == OrganizationId && x.id == ProjectId)
            .Include(x => x.boards)
            .Select(x => new ProjectGET(x))
            .ToList();
        return !cmp.Any() ? NotFound() : Ok(cmp.Single());
    }
    
    #endregion

    #region DELETE
    [HttpDelete("Projects/{ProjectId}")]
    public ActionResult DeleteProjects(ulong OrganizationId, ulong ProjectId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(OrganizationId))
            return NotFound();
        using var ctx = DbContexts.Get<ProjectContext>();
        var cmp = ContextIncluder.DeleteProject(ctx).Where(x => x.id == ProjectId && x.OrganizationId == OrganizationId);
        ctx.Projects.RemoveRange(cmp);
        ctx.SaveChanges();
        return Ok();
    }
    #endregion

    #region POST
    [HttpPost("Projects")]
    public ActionResult<ProjectGET> PostProjects(ulong OrganizationId, ProjectDTO project)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(OrganizationId))
            return NotFound();
        
        using var ctx = DbContexts.Get<ProjectContext>();
        var obj = new Project
        {
            name = project.Name,
            OrganizationId = OrganizationId
        };
        ctx.Projects.Add(obj);
        ctx.SaveChanges();
        return Ok(new ProjectGET(obj));
    }
    #endregion

    #region PATCH
    [HttpPatch("Projects/{ProjectId}")]
    public ActionResult<ProjectGET> PatchProject(ulong OrganizationId, ulong ProjectId, [FromBody] JsonPatchDocument<Project> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsProjectPathCorrect(OrganizationId))
            return NotFound();
        using var ctx = DbContexts.Get<ProjectContext>();
        var obj = ContextIncluder.GetProject(ctx)
            .Include(x => x.boards)
            .Where(x => x.id == ProjectId && x.OrganizationId == OrganizationId)
            .ToList();
        patch.ApplyTo(obj.Single());
        ctx.SaveChanges();
        
        return Ok(new ProjectGET(obj.Single()));
    }
    #endregion
}
