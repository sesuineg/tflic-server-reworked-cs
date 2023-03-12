using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Server.Controllers.DTO.GET;
using Server.Controllers.DTO.POST;
using Server.Controllers.Service;
using Server.Models.Contexts;
using Task = Server.Models.Organization.Project.Task;

namespace Server.Controllers;

#if AUTH
using Models.Authentication;
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("Organizations/{OrganizationId}/Projects/{ProjectId}/Boards/{BoardId}/Columns/{ColumnId}")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class TaskController : ControllerBase
{
    private readonly ILogger<TaskController> _logger;

    public TaskController(ILogger<TaskController> logger)
    {
        _logger = logger;
    }

    #region GET
    [HttpGet("Tasks")]
    public ActionResult<ICollection<TaskGET>> GetTasks(ulong OrganizationId, ulong ProjectId, ulong BoardId, ulong ColumnId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(OrganizationId, ProjectId, BoardId, ColumnId))
            return NotFound();
        using var ctx = DbContexts.Get<TaskContext>();
        var cmp = ContextIncluder.GetTask(ctx)
            //.Include(x => x.Components)
            .Where(x => x.ColumnId == ColumnId)
            .Select(x => new TaskGET(x))
            .ToList();
        return Ok(cmp);
    }
    
    [HttpGet("Tasks/{TaskId}")]
    public ActionResult<TaskGET> GetTask(ulong OrganizationId, ulong ProjectId, ulong BoardId, ulong ColumnId, ulong TaskId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(OrganizationId, ProjectId, BoardId, ColumnId))
            return NotFound();
        using var ctx = DbContexts.Get<TaskContext>();
        var cmp = ContextIncluder.GetTask(ctx)
            //.Include(x => x.Components)
            .Where(x => x.ColumnId == ColumnId && x.Id == TaskId)
            .Select(x => new TaskGET(x))
            .ToList();
        return !cmp.Any() ? NotFound() : Ok(cmp.Single());
    }
    #endregion

    #region DELETE
    [HttpDelete("Tasks/{TaskId}")]
    public ActionResult DeleteTask(ulong OrganizationId, ulong ProjectId, ulong BoardId, ulong ColumnId, ulong TaskId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(OrganizationId, ProjectId, BoardId, ColumnId))
            return NotFound();
        using var ctx = DbContexts.Get<TaskContext>();
        var cmp = ContextIncluder.DeleteTask(ctx)
            .Where(x => x.Id == TaskId && x.ColumnId == ColumnId);
        ctx.Tasks.RemoveRange(cmp);
        ctx.SaveChanges();
        return Ok();
    }
    
    #endregion

    #region POST
    [HttpPost("Tasks")]
    public ActionResult<TaskGET> PostTask(ulong OrganizationId, ulong ProjectId, ulong BoardId, ulong ColumnId,
        TaskDTO taskDto)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(OrganizationId, ProjectId, BoardId, ColumnId))
            return NotFound();
        
        using var ctx = DbContexts.Get<TaskContext>();
        var obj = new Models.Organization.Project.Task()
        {
            
            Name = taskDto.Name,
            Position = taskDto.Position,
            Description = taskDto.Description,
            CreationTime = taskDto.CreationTime,
            Status = taskDto.Status,
            ColumnId = ColumnId,
            ExecutorId = taskDto.id_executor,
            Deadline = taskDto.Deadline,
            priority = taskDto.priority,
            EstimatedTime = taskDto.EstimatedTime
            
        };
        ctx.Tasks.Add(obj);
        ctx.SaveChanges();
        return Ok(new TaskGET(obj));
    }
    #endregion
    
    #region PATCH
    [HttpPatch("Tasks/{TaskId}")]
    public ActionResult<TaskGET> PatchTask(ulong OrganizationId, ulong ProjectId, ulong BoardId, ulong ColumnId, ulong TaskId,
        [FromBody] JsonPatchDocument<Models.Organization.Project.Task> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(OrganizationId, ProjectId, BoardId, ColumnId))
            return NotFound();
        using var ctx = DbContexts.Get<TaskContext>();
        var obj = ContextIncluder.GetTask(ctx)
            .Where(x => x.Id == TaskId && x.ColumnId == ColumnId).ToList();
        patch.ApplyTo(obj.Single());
        ctx.SaveChanges();
        
        return Ok(new TaskGET(obj.Single()));
    }
    #endregion
}