using TFlic.Models.Domain.Organization.Accounts;

namespace TFlic.Controllers.Version2.DTO;

// todo для dto использовать mapster / automapper
public record UserGroupDto
{
    public UserGroupDto(UserGroup userGroup)
    {
        GlobalId = userGroup.GlobalId;
        LocalId = userGroup.LocalId;
        OrganizationId = userGroup.OrganizationId;
        Name = userGroup.Name;
        foreach (var account in userGroup.Accounts) { Accounts.Add(account.Id); }
    }
    
    public ulong GlobalId { get; }
    public short LocalId { get; }
    public ulong OrganizationId { get; }
    public string Name { get; }
    public List<ulong> Accounts { get; } = new();
}
