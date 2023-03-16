using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TFlic.Controllers.Version2.DTO.GET;
using TFlic.Controllers.Version2.DTO.POST;
using TFlic.Controllers.Version2.Service;
using TFlic.Models.Contexts;

namespace TFlic.Controllers.Version2;

#if AUTH
using Models.Authentication;
using Microsoft.AspNetCore.Authorization;
[Authorize]
#endif
[ApiController]
[Route("api/v2/organizations/{organizationId}/projects/{projectId}/boards/{boardId}/columns/{columnId}")]
public class TaskController : ControllerBase
{
    private readonly ILogger<TaskController> _logger;

    public TaskController(ILogger<TaskController> logger)
    {
        _logger = logger;
    }

    #region GET
    [HttpGet("tasks")]
    public ActionResult<ICollection<TaskGet>> GetTasks(ulong organizationId, ulong projectId, ulong boardId, ulong columnId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(organizationId, projectId, boardId, columnId))
            return NotFound();
        using var ctx = DbContexts.Get<TaskContext>();
        var cmp = ContextIncluder.GetTask(ctx)
            //.Include(x => x.Components)
            .Where(x => x.ColumnId == columnId)
            .Select(x => new TaskGet(x))
            .ToList();
        return Ok(cmp);
    }
    
    [HttpGet("tasks/{taskId}")]
    public ActionResult<TaskGet> GetTask(ulong organizationId, ulong projectId, ulong boardId, ulong columnId, ulong taskId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId, allowNoRole: true)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(organizationId, projectId, boardId, columnId))
            return NotFound();
        using var ctx = DbContexts.Get<TaskContext>();
        var cmp = ContextIncluder.GetTask(ctx)
            //.Include(x => x.Components)
            .Where(x => x.ColumnId == columnId && x.Id == taskId)
            .Select(x => new TaskGet(x))
            .ToList();
        return !cmp.Any() ? NotFound() : Ok(cmp.Single());
    }
    #endregion

    #region DELETE
    [HttpDelete("tasks/{taskId}")]
    public ActionResult DeleteTask(ulong organizationId, ulong projectId, ulong boardId, ulong columnId, ulong taskId)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(organizationId, projectId, boardId, columnId))
            return NotFound();
        using var ctx = DbContexts.Get<TaskContext>();
        var cmp = ContextIncluder.DeleteTask(ctx)
            .Where(x => x.Id == taskId && x.ColumnId == columnId);
        ctx.Tasks.RemoveRange(cmp);
        ctx.SaveChanges();
        return Ok();
    }
    
    #endregion

    #region POST
    [HttpPost("tasks")]
    public ActionResult<TaskGet> PostTask(ulong organizationId, ulong projectId, ulong boardId, ulong columnId,
        TaskDto taskDto)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(organizationId, projectId, boardId, columnId))
            return NotFound();
        
        using var ctx = DbContexts.Get<TaskContext>();
        var obj = new Models.Organization.Project.Task()
        {
            
            Name = taskDto.Name,
            Position = taskDto.Position,
            Description = taskDto.Description,
            CreationTime = taskDto.CreationTime,
            Status = taskDto.Status,
            ColumnId = columnId,
            ExecutorId = taskDto.IdExecutor,
            Deadline = taskDto.Deadline,
            priority = taskDto.Priority,
            EstimatedTime = taskDto.EstimatedTime
            
        };
        ctx.Tasks.Add(obj);
        ctx.SaveChanges();
        return Ok(new TaskGet(obj));
    }
    #endregion
    
    #region PATCH
    [HttpPatch("tasks/{taskId}")]
    public ActionResult<TaskGet> PatchTask(ulong organizationId, ulong projectId, ulong boardId, ulong columnId, ulong taskId,
        [FromBody] JsonPatchDocument<Models.Organization.Project.Task> patch)
    {
#if AUTH
        var token = TokenProvider.GetToken(Request);
        if (!AuthenticationManager.Authorize(token, OrganizationId)) { return Forbid(); }
#endif
        if (!PathChecker.IsTaskPathCorrect(organizationId, projectId, boardId, columnId))
            return NotFound();
        using var ctx = DbContexts.Get<TaskContext>();
        var obj = ContextIncluder.GetTask(ctx)
            .Where(x => x.Id == taskId && x.ColumnId == columnId).ToList();
        patch.ApplyTo(obj.Single());
        ctx.SaveChanges();
        
        return Ok(new TaskGet(obj.Single()));
    }
    #endregion
}