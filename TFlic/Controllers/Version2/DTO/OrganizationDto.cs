using TFlic.Models.Domain.Organization;

namespace TFlic.Controllers.Version2.DTO;

using ModelOrganization = Organization;

public record OrganizationDto
{
    public OrganizationDto(ModelOrganization organization)
    {
        Id = organization.Id;
        Name = organization.Name;
        Description = organization.Description;
        UserGroups = organization.GetUserGroups().Select(ug => new UserGroupIdSet(ug.GlobalId, ug.LocalId)).ToList();
        ActiveProjects = organization.Projects.Where(prj => !prj.IsArchived).Select(ug => ug.Id).ToList();
        ArchivedProjects = organization.Projects.Where(prj => prj.IsArchived).Select(ug => ug.Id).ToList();
    }
    
    public ulong Id { get; }
    public string Name { get; }
    public string? Description { get; }
    public List<UserGroupIdSet> UserGroups { get; }
    public List<ulong> ActiveProjects { get; }
    public List<ulong> ArchivedProjects { get; }
}

public record UserGroupIdSet(ulong GlobalId, short LocalId);