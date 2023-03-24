namespace TFlic.Models.Domain.Organization.Project;
public class Project
{
    public ulong Id { get; set; }

    public ulong OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsArchived { get; set; }

    public ICollection<Board>? Boards { get; set; }
}