using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TFlic.Controllers.Version2.DTO.POST;
using TFlic.Models.Organization.Project.Component;
using TFlic.Controllers.Version2.DTO.GET;
using TFlic.Controllers.Version2.Service;
using TFlic.Models.Contexts;
using ComponentDto = TFlic.Controllers.Version2.DTO.POST.ComponentDto;

namespace TFlic.Controllers.Version2;

#if AUTH
using Models.Authentication;
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("api/v2/organizations/{organizationId}/projects/{projectId}/boards/{boardId}/columns/{columnId}/tasks/{taskId}")]
public class ComponentController : ControllerBase
{
    public ComponentController(ComponentContext componentContext)
    {
        _componentContext = componentContext;
    }

    #region GET
    [HttpGet("components")]
    public ActionResult<ICollection<ComponentGet>> GetComponents(ulong organizationId, ulong projectId, ulong boardId, ulong columnId, ulong taskId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsComponentPathCorrect(organizationId, projectId, boardId, columnId, taskId))
            return NotFound();

        var cmp = ContextIncluder.GetComponent(_componentContext)
            .Where(x => x.task_id == taskId)
            .Select(x => new ComponentGet(x))
            .ToList();
        return Ok(cmp);
    }

    [HttpGet("components/{componentId}")]
    public ActionResult<ComponentGet> GetComponent(ulong organizationId, ulong projectId, ulong boardId, ulong columnId, ulong taskId,
        ulong componentId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsComponentPathCorrect(organizationId, projectId, boardId, columnId, taskId))
            return NotFound();

        var cmp = ContextIncluder.GetComponent(_componentContext)
            .Where(x => x.task_id == taskId && x.id == componentId)
            .Select(x => new ComponentGet(x))
            .ToList();
        return !cmp.Any() ? NotFound() : Ok(cmp.Single());
    }
    #endregion

    #region DELETE
    [HttpDelete("components/{componentId}")]
    public ActionResult DeleteComponent(ulong organizationId, ulong projectId, ulong boardId, ulong columnId, ulong taskId,
        ulong componentId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsComponentPathCorrect(organizationId, projectId, boardId, columnId, taskId))
            return NotFound();

        var cmp = ContextIncluder.DeleteComponent(_componentContext).Where(x => x.id == componentId);
        _componentContext.Components.RemoveRange(cmp);
        _componentContext.SaveChanges();
        return Ok();
    }
    #endregion

    #region POST
    [HttpPost("components")]
    public ActionResult<ComponentGet> PostComponent(ulong organizationId, ulong projectId, ulong boardId, ulong columnId, ulong taskId,
        DTO.POST.ComponentDto componentDto)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsComponentPathCorrect(organizationId, projectId, boardId, columnId, taskId))
            return NotFound();
        
        var obj = new Models.Organization.Project.Component.ComponentDto()
        {
            name = componentDto.Name,
            position = componentDto.Position,
            component_type_id = componentDto.ComponentTypeId,
            value = componentDto.Value,
            task_id = taskId
        };
        _componentContext.Components.Add(obj);
        _componentContext.SaveChanges();
        return Ok(new ComponentGet(obj));
    }
    #endregion
    
    #region PATCH
    [HttpPatch("components/{componentId}")]
    public ActionResult<ComponentGet> PatchComponent(ulong organizationId, ulong projectId, ulong boardId, ulong columnId,ulong taskId,
        ulong componentId, [FromBody] JsonPatchDocument<Models.Organization.Project.Component.ComponentDto> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsComponentPathCorrect(organizationId, projectId, boardId, columnId, taskId))
            return NotFound();

        var obj = ContextIncluder.GetComponent(_componentContext).Where(x => x.id == componentId).ToList();
        patch.ApplyTo(obj.Single());
        _componentContext.SaveChanges();
        
        return Ok(new ComponentGet(obj.Single()));
    }
    #endregion
    
    
    
    private readonly ComponentContext _componentContext;
}