using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFlic.Controllers.Version2.DTO.GET;
using TFlic.Controllers.Version2.DTO.POST;
using TFlic.Models.Domain.Organization.Project;
using TFlic.Models.Services.Contexts;

namespace TFlic.Controllers.Version2;

[Authorize]
[ApiController]
[Route("api/v2")]
public class ColumnsController : ControllerBase
{
    public ColumnsController(TFlicDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet("boards/{boardId}/columns")]
    public ActionResult<IEnumerable<ColumnGet>> GetColumns(ulong boardId)
    {
        // todo вынести _dbContext.Boards в отдельную переменную _boards 
        var board = _dbContext.Boards.SingleOrDefault(board => board.Id == boardId);
        if (board is null)
            return NotFound();

        var columns = _dbContext.Columns
            .Where(column => column.BoardId == boardId)
            .Select(column => new ColumnGet(column));
        
        return Ok(columns);
    }

    [HttpGet("columns/{columnId}")]
    public ActionResult<ColumnGet> GetColumn(ulong columnId)
    {
        var column = _dbContext.Columns
            .Where(column => column.Id == columnId)
            .Include(column => column.Tasks)
            .SingleOrDefault();
            
        return column is not null
            ? new ColumnGet(column)
            : NotFound();
    }

    [HttpDelete("columns/{columnId}")]
    public ActionResult DeleteColumn(ulong columnId)
    {
        var columnToDelete = _dbContext.Columns
            .SingleOrDefault(column => column.Id == columnId);

        if (columnToDelete is null)
            return NotFound($"column with id {columnId} doesn't exist");

        _dbContext.Columns.Remove(columnToDelete);
        _dbContext.SaveChanges();

        return Ok();
    }

    [HttpPost("boards/{boardId}/columns")]
    public ActionResult<ColumnGet> CreateColumn(ulong boardId, ColumnDto column)
    {
        var board = _dbContext.Boards.SingleOrDefault(board => board.Id == boardId);
        if (board is null)
            return NotFound();
        
        var newColumn = new Column
        {
            Name = column.Name,
            Position = column.Position,
            BoardId = boardId
        };
        _dbContext.Columns.Add(newColumn);
        _dbContext.SaveChanges();
        
        return Ok(new ColumnGet(newColumn));
    }
    
    [HttpPatch("columns/{columnId}")]
    public ActionResult<ColumnGet> PatchColumn(ulong columnId, [FromBody] JsonPatchDocument<Column> patch)
    {
        var columnToPatch = _dbContext.Columns.SingleOrDefault(column => column.Id == columnId);
        if (columnToPatch is null)
            return NotFound();

        patch.ApplyTo(columnToPatch);
        _dbContext.SaveChanges();
        
        return Ok(new ColumnGet(columnToPatch));
    }



    private readonly TFlicDbContext _dbContext;
}