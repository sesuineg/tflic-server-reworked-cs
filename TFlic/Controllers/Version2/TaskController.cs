using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TFlic.Controllers.Version2.DTO.GET;
using TFlic.Controllers.Version2.DTO.POST;
using TFlic.Models.Contexts;
using ModelTask = TFlic.Models.Organization.Project.Task;

namespace TFlic.Controllers.Version2;

[Authorize]
[ApiController]
[Route("api/v2")]
public class TaskController : ControllerBase
{
    public TaskController(TaskContext taskContext, ColumnContext columnContext)
    {
        _taskContext = taskContext;
        _columnContext = columnContext;
    }

    // todo продумать возврат задач не для колонки, а для всего проекта
    [HttpGet("columns/{columnId}/tasks")]
    public ActionResult<IEnumerable<TaskGet>> GetTasks(ulong columnId)
    {
        var column = _columnContext.Columns.SingleOrDefault(column => column.Id == columnId);
        if (column is null)
            return NotFound();

        var tasks = _taskContext.Tasks
            .Where(task => task.ColumnId == columnId)
            .Select(task => new TaskGet(task));

        return Ok(tasks);
    }
    
    [HttpGet("tasks/{taskId}")]
    public ActionResult<TaskGet> GetTask(ulong taskId)
    {
        var task = _taskContext.Tasks.SingleOrDefault(task => task.Id == taskId);

        return task is not null
            ? new TaskGet(task)
            : NotFound();
    }

    [HttpDelete("tasks/{taskId}")]
    public ActionResult DeleteTask(ulong taskId)
    {
        var taskToDelete = _taskContext.Tasks.SingleOrDefault(task => task.Id == taskId);
        if (taskToDelete is null)
            return NotFound();

        _taskContext.Tasks.Remove(taskToDelete);
        _taskContext.SaveChanges();

        return Ok();
    }
    

    [HttpPost("columns/{columnId}/tasks")]
    public ActionResult<TaskGet> CreateTask(ulong columnId,
        TaskDto taskDto)
    {
        var column = _columnContext.Columns.SingleOrDefault(column => column.Id == columnId);
        if (column is null)
            return NotFound();

        var newTask = new ModelTask
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
        
        _taskContext.Tasks.Add(newTask);
        _taskContext.SaveChanges();
        return Ok(new TaskGet(newTask));
    }
    
    [HttpPatch("tasks/{taskId}")]
    public ActionResult<TaskGet> PatchTask(ulong taskId,
        [FromBody] JsonPatchDocument<ModelTask> patch)
    {
        var taskToPatch = _taskContext.Tasks.SingleOrDefault(task => task.Id == taskId);
        if (taskToPatch is null)
            return NotFound();

        patch.ApplyTo(taskToPatch);
        _taskContext.SaveChanges();
        
        return Ok(new TaskGet(taskToPatch));
    }
    
    
    
    private readonly TaskContext _taskContext;
    private readonly ColumnContext _columnContext;
}