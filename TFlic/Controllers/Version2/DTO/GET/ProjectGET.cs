using TFlic.Models.Domain.Organization.Project;

namespace TFlic.Controllers.Version2.DTO.GET;

public class ProjectGet
{
    public ProjectGet(Project project)
    {
        Id = project.Id;
        Name = project.Name;
        if (project.Boards == null) return;
        foreach (var board in project.Boards) { Boards.Add(board.Id); }
    }
    public ulong Id { get; set; }
    public string Name { get; set; }
    public List<ulong> Boards { get; } = new();
}