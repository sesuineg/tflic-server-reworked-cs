namespace TFlic.Controllers.Version2.DTO.POST;

public record ColumnDto
{
    public string Name { get; init; }
    public int Position { get; init; }
    public int LimitOfTask { get; init; }
}