namespace Server.Controllers.Version2.DTO.POST;

public record ComponentDto
{
    public ulong ComponentTypeId { get; init; }
    public int Position { get; init; }
    public string Name { get; init; }
    public string Value { get; init; }
}