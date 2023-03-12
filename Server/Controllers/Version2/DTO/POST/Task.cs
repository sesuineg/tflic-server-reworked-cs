namespace Server.Controllers.Version2.DTO.POST;

public record TaskDto
{
    public int Position { get; init; }
    
    public uint EstimatedTime { get; init; }
    public string Name { get; init; }

    public string Description { get; init; }
    public DateTime CreationTime { get; init; }
    public string Status { get; init; }

    public ulong IdExecutor { get; init; }
    
    public uint Priority { get; init; }
    public DateTime Deadline { get; init; }
}