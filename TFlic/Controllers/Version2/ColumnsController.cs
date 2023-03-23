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
public class ColumnsController : ControllerBase
{
    public ColumnsController(ColumnContext columnContext, BoardContext boardContext)
    {
        _columnContext = columnContext;
        _boardContext = boardContext;
    }

    // todo удалить регионы
    #region GET
    [HttpGet("boards/{boardId}/columns")]
    public ActionResult<IEnumerable<ColumnGet>> GetColumns(ulong boardId)
    {
// todo удалить секции AUTH
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif

        // todo вынести _boardContext.Boards в отдельную переменную _boards 
        var board = _boardContext.Boards.SingleOrDefault(board => board.id == boardId);
        if (board is null)
            return NotFound();

        var columns = _columnContext.Columns
            .Where(column => column.BoardId == boardId)
            .Select(column => new ColumnGet(column));
        
        return Ok(columns);
    }

    [HttpGet("columns/{columnId}")]
    public ActionResult<ColumnGet> GetColumn(ulong columnId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif

        var column = _columnContext.Columns
            .SingleOrDefault(column => column.Id == columnId);
            
        return column is not null
            ? new ColumnGet(column)
            : NotFound();
    }
    #endregion

    #region DELETE
    [HttpDelete("columns/{columnId}")]
    public ActionResult DeleteColumn(ulong columnId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif

        var columnToDelete = _columnContext.Columns
            .SingleOrDefault(column => column.Id == columnId);

        if (columnToDelete is null)
            return NotFound($"column with id {columnId} doesn't exist");

        _columnContext.Columns.Remove(columnToDelete);
        _columnContext.SaveChanges();

        return Ok();
    }
    #endregion

    #region POST
    [HttpPost("boards/{boardId}/columns")]
    public ActionResult<ColumnGet> CreateColumn(ulong boardId, ColumnDto column)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif

        var board = _boardContext.Boards.SingleOrDefault(board => board.id == boardId);
        if (board is null)
            return NotFound();
        
        var newColumn = new Column
        {
            Name = column.Name,
            Position = column.Position,
            LimitOfTask = column.LimitOfTask,
            BoardId = boardId
        };
        _columnContext.Columns.Add(newColumn);
        _columnContext.SaveChanges();
        
        return Ok(new ColumnGet(newColumn));
    }
    #endregion
    
    #region PATCH
    [HttpPatch("columns/{columnId}")]
    public ActionResult<ColumnGet> PatchColumn(ulong columnId,
        [FromBody] JsonPatchDocument<Column> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif

        var columnToPatch = _columnContext.Columns.SingleOrDefault(column => column.Id == columnId);
        if (columnToPatch is null)
            return NotFound();

        patch.ApplyTo(columnToPatch);
        _columnContext.SaveChanges();
        
        return Ok(new ColumnGet(columnToPatch));
    }
    #endregion



    private readonly ColumnContext _columnContext;
    private readonly BoardContext _boardContext;
}