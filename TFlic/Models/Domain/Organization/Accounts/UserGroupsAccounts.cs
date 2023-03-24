namespace TFlic.Models.Domain.Organization.Accounts;

public class UserGroupsAccounts
{
    public ulong Id { get; set; }
    
    public ulong UserGroupId { get; set; }
    public UserGroup UserGroup { get; set; } = null!;
    
    public ulong AccountId { get; set; }
    public Account Account { get; set; } = null!;
}