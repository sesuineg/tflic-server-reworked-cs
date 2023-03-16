namespace TFlic.Controllers.Version2.DTO.GET;

public class TaskGet
{
    public TaskGet(Models.Organization.Project.Task task)
    {
        Id = task.Id;
        IdColumn = task.ColumnId;
        Position = task.Position;
        Name = task.Name;
        CreationTime = task.CreationTime;
        Description = task.Description;
        Status = task.Status;
        IdExecutor = task.ExecutorId;
        Deadline = task.Deadline;
        Priority = task.priority;
        EstimatedTime = task.EstimatedTime;
        if (task.Components == null) return;
        foreach (var component in task.Components) { Components.Add(component.id); }
    }
    
    public ulong Id { get; init; }
    public ulong IdColumn { get; init; }

    public int Position { get; set; }
    
    public uint EstimatedTime { get; set; }

    public string Name { get; set; }

    public ulong Priority { get; init; }

    public string Description { get; set; }
    public ulong IdExecutor { get; set; }
    public List<ulong> Authors {get;} = new();
    public DateTime CreationTime { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; }
    //public ICollection<Tag> Tags { get; }

    public List<ulong> Components { get; } = new();
}