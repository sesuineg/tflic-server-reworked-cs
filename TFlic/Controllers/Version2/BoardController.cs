using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFlic.Controllers.Version2.DTO.GET;
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
[Route("api/v2/organizations/{organizationId}/projects/{projectId}")]
public class BoardController : ControllerBase
{
    public BoardController(BoardContext boardContext)
    {
        _boardContext = boardContext;
    }

    #region GET
    [HttpGet("boards")]
    public ActionResult<ICollection<BoardGet>> GetBoards(ulong organizationId, ulong projectId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsBoardPathCorrect(organizationId, projectId))
            return NotFound();

        var cmp = ContextIncluder.GetBoard(_boardContext)
            .Include(x => x.Columns)
            .Where(x => x.ProjectId == projectId)
            .Select(x => new BoardGet(x))
            .ToList();
        return Ok(cmp);
    }
    
    [HttpGet("boards/{boardId}")]
    public ActionResult<BoardGet> GetBoard(ulong organizationId, ulong projectId, ulong boardId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsBoardPathCorrect(organizationId, projectId))
            return NotFound();

        var cmp = ContextIncluder.GetBoard(_boardContext)
            .Include(x => x.Columns)
            .Where(x => x.ProjectId == projectId && x.id == boardId)
            .Select(x => new BoardGet(x))
            .ToList();
        return !cmp.Any() ? NotFound() : Ok(cmp.Single());
    }
    #endregion

    #region DELETE
    [HttpDelete("boards/{boardId}")]
    public ActionResult DeleteBoards(ulong organizationId, ulong projectId, ulong boardId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsBoardPathCorrect(organizationId, projectId))
            return NotFound();

        var cmp = ContextIncluder.DeleteBoard(_boardContext).Where(x => x.id == boardId && x.ProjectId == projectId);
        _boardContext.Boards.RemoveRange(cmp);
        _boardContext.SaveChanges();
        return Ok();
    }
    #endregion
    
    #region POST
    [HttpPost("boards")]
    public ActionResult<BoardGet> PostBoards(ulong organizationId, ulong projectId, DTO.POST.BoardDto board)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsBoardPathCorrect(organizationId, projectId))
            return NotFound();

        var obj = new Board
        {
            Name = board.Name,
            ProjectId = projectId
        };
        obj.Columns.Add(new Column{Position = 0, LimitOfTask = 0, Name = "backlog"});
        _boardContext.Boards.Add(obj);
        _boardContext.SaveChanges();
        return Ok(new BoardGet(obj));
    }

    #endregion
    
    #region PATCH
    [HttpPatch("boards/{boardId}")]
    public ActionResult<BoardGet> PatchBoard(ulong organizationId, ulong projectId, ulong boardId, [FromBody] JsonPatchDocument<Board> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsBoardPathCorrect(organizationId, projectId))
            return NotFound();

        var obj = ContextIncluder.GetBoard(_boardContext).Where(x => x.id == boardId && x.ProjectId == projectId).ToList();
        patch.ApplyTo(obj.Single());
        _boardContext.SaveChanges();
        
        return Ok(new BoardGet(obj.Single()));
    }
    #endregion
    
    
    
    private readonly BoardContext _boardContext;
}
