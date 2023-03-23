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
[Route("api/v2/organizations/{organizationId}/projects/{projectId}/boards/{boardId}")]
public class ColumnController : ControllerBase
{
    public ColumnController(ColumnContext columnContext)
    {
        _columnContext = columnContext;
    }

    #region GET
    [HttpGet("columns")]
    public ActionResult<ICollection<ColumnGet>> GetColumns(ulong organizationId, ulong projectId, ulong boardId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsColumnPathCorrect(organizationId, projectId, boardId))
            return NotFound();

        var cmp = ContextIncluder.GetColumn(_columnContext)
            .Include(x => x.Tasks)
            .Where(x => x.BoardId == boardId)
            .Select(x => new ColumnGet(x))
            .ToList();
        return Ok(cmp);
    }

    [HttpGet("columns/{columnId}")]
    public ActionResult<ColumnGet> GetColumn(ulong organizationId, ulong projectId, ulong boardId, ulong columnId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsColumnPathCorrect(organizationId, projectId, boardId))
            return NotFound();

        var cmp = ContextIncluder.GetColumn(_columnContext)
            .Include(x => x.Tasks)
            .Where(x => x.BoardId == boardId && x.Id == columnId)
            .Select(x => new ColumnGet(x))
            .ToList();
        return !cmp.Any() ? NotFound() : Ok(cmp.Single());
    }
    #endregion

    #region DELETE
    [HttpDelete("columns/{columnId}")]
    public ActionResult DeleteColumn(ulong organizationId, ulong projectId, ulong boardId, ulong columnId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsColumnPathCorrect(organizationId, projectId, boardId))
            return NotFound();

        var cmp = ContextIncluder.DeleteColumn(_columnContext).Where(x => x.Id == columnId && x.BoardId == boardId);
        _columnContext.Columns.RemoveRange(cmp);
        _columnContext.SaveChanges();
        return Ok();
    }
    #endregion

    #region POST
    [HttpPost("columns")]
    public ActionResult<ColumnGet> PostColumn(ulong organizationId, ulong projectId, ulong boardId, ColumnDto column)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsColumnPathCorrect(organizationId, projectId, boardId))
            return NotFound();
        
        var obj = new Column()
        {
            
            Name = column.Name,
            Position = column.Position,
            LimitOfTask = column.LimitOfTask,
            BoardId = boardId
        };
        _columnContext.Columns.Add(obj);
        _columnContext.SaveChanges();
        return Ok(new ColumnGet(obj));
    }
    #endregion
    
    #region PATCH
    [HttpPatch("columns/{columnId}")]
    public ActionResult<ColumnGet> PatchColumn(ulong organizationId, ulong projectId, ulong boardId, ulong columnId,
        [FromBody] JsonPatchDocument<Column> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsColumnPathCorrect(organizationId, projectId, boardId))
            return NotFound();

        var obj = ContextIncluder.GetColumn(_columnContext).Where(x => x.Id == columnId && x.BoardId == boardId).ToList();
        patch.ApplyTo(obj.Single());
        _columnContext.SaveChanges();
        
        return Ok(new ColumnGet(obj.Single()));
    }
    #endregion



    private readonly ColumnContext _columnContext;
}