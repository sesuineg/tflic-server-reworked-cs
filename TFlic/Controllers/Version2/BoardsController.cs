using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFlic.Controllers.Version2.DTO.GET;
using TFlic.Controllers.Version2.Service;
using TFlic.Models.Contexts;
using TFlic.Models.Organization.Project;
using TFlic.Controllers.Version2.DTO.POST;

namespace TFlic.Controllers.Version2;

#if AUTH
using Models.Authentication;
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("api/v2")]
public class BoardsController : ControllerBase
{
    public BoardsController(BoardContext boardContext, ProjectContext projectContext)
    {
        _boardContext = boardContext;
        _projectContext = projectContext;
    }

    #region GET
    [HttpGet("projects/{projectId}/boards")]
    public ActionResult<IEnumerable<BoardGet>> GetBoards(ulong projectId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif

        var project = _projectContext.Projects.SingleOrDefault(project => project.id == projectId);
        if (project is null)
            return NotFound();

        var boards = _boardContext.Boards
            .Where(board => board.ProjectId == projectId)
            .Select(board => new BoardGet(board));
        
        return Ok(boards);
    }
    
    [HttpGet("boards/{boardId}")]
    public ActionResult<BoardGet> GetBoard(ulong boardId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        var board = _boardContext.Boards
            .Where(board => board.id == boardId)
            .Include(x => x.Project)
            .ThenInclude(x => x.Organization)
            .SingleOrDefault();
            
        return board is not null
            ? Ok(new BoardGet(board))
            : NotFound();
    }
    #endregion

    #region DELETE
    [HttpDelete("boards/{boardId}")]
    public ActionResult DeleteBoards(ulong boardId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif

        var boardToDelete = _boardContext.Boards
            .SingleOrDefault(board => board.id == boardId);
        
        if (boardToDelete is null) 
            return NotFound($"board with id {boardId} doesn't exist");
        
        _boardContext.Boards.Remove(boardToDelete);
        _boardContext.SaveChanges();
        
        return Ok();
    }
    #endregion
    
    #region POST
    [HttpPost("projects/{projectId}/boards")]
    public ActionResult<BoardGet> CreateBoard(ulong projectId, BoardDto board)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif

        var project = _projectContext.Projects
            .SingleOrDefault(project => project.id == projectId);
        if (project is null)
            return NotFound();
        
        var newBoard = new Board
        {
            Name = board.Name,
            ProjectId = projectId
        };
        newBoard.Columns.Add(new Column{Position = 0, LimitOfTask = 0, Name = "backlog"});
        _boardContext.Boards.Add(newBoard);
        _boardContext.SaveChanges();
        
        return Ok(new BoardGet(newBoard));
    }
    #endregion
    
    #region PATCH
    [HttpPatch("boards/{boardId}")]
    public ActionResult<BoardGet> PatchBoard(ulong boardId, [FromBody] JsonPatchDocument<Board> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        
        var boardToPatch = _boardContext.Boards.SingleOrDefault(board => board.id == boardId);
        if (boardToPatch is null)
            return NotFound();
        
        patch.ApplyTo(boardToPatch);
        _boardContext.SaveChanges();
        
        return Ok(new BoardGet(boardToPatch));
    }
    #endregion
    
    
    
    private readonly BoardContext _boardContext;
    private readonly ProjectContext _projectContext;
}
