namespace TFlic.Controllers.Version2.DTO.GET;

public class ComponentGet
{
    
    public ComponentGet(Models.Organization.Project.Component.ComponentDto componentDto)
    {
        Id = componentDto.id;
        Name = componentDto.name;
        Position = componentDto.position;
        ComponentTypeId = componentDto.component_type_id;
        Value = componentDto.value;
    }
    public ulong Id { get;}

    public ulong ComponentTypeId { get;}
    
    public int Position { get;}
    public string Name { get;}
    public string Value { get;}
}