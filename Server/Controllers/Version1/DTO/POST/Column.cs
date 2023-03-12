namespace Server.Controllers.Version1.DTO.POST;

public record ColumnDTO
{
    public string Name { get; init; }
    public int Position { get; init; }
    public int LimitOfTask { get; init; }
}