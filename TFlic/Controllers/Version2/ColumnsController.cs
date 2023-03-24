using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TFlic.Controllers.Version2.DTO.GET;
using TFlic.Controllers.Version2.DTO.POST;
using TFlic.Models.Contexts;
using TFlic.Models.Organization.Project;

namespace TFlic.Controllers.Version2;

[Authorize]
[ApiController]
[Route("api/v2")]
public class ColumnsController : ControllerBase
{
    public ColumnsController(ColumnContext columnContext, BoardContext boardContext)
    {
        _columnContext = columnContext;
        _boardContext = boardContext;
    }
    
    [HttpGet("boards/{boardId}/columns")]
    public ActionResult<IEnumerable<ColumnGet>> GetColumns(ulong boardId)
    {
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
        var column = _columnContext.Columns
            .SingleOrDefault(column => column.Id == columnId);
            
        return column is not null
            ? new ColumnGet(column)
            : NotFound();
    }

    [HttpDelete("columns/{columnId}")]
    public ActionResult DeleteColumn(ulong columnId)
    {
        var columnToDelete = _columnContext.Columns
            .SingleOrDefault(column => column.Id == columnId);

        if (columnToDelete is null)
            return NotFound($"column with id {columnId} doesn't exist");

        _columnContext.Columns.Remove(columnToDelete);
        _columnContext.SaveChanges();

        return Ok();
    }

    [HttpPost("boards/{boardId}/columns")]
    public ActionResult<ColumnGet> CreateColumn(ulong boardId, ColumnDto column)
    {
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
    
    [HttpPatch("columns/{columnId}")]
    public ActionResult<ColumnGet> PatchColumn(ulong columnId, [FromBody] JsonPatchDocument<Column> patch)
    {
        var columnToPatch = _columnContext.Columns.SingleOrDefault(column => column.Id == columnId);
        if (columnToPatch is null)
            return NotFound();

        patch.ApplyTo(columnToPatch);
        _columnContext.SaveChanges();
        
        return Ok(new ColumnGet(columnToPatch));
    }



    private readonly ColumnContext _columnContext;
    private readonly BoardContext _boardContext;
}