namespace TFlic.Controllers.Version2.DTO.GET;

public class ProjectGet
{
    public ProjectGet(Models.Organization.Project.Project project)
    {
        Id = project.id;
        Name = project.name;
        if (project.boards == null) return;
        foreach (var board in project.boards) { Boards.Add(board.id); }
    }
    public ulong Id { get; set; }
    public string Name { get; set; }
    public List<ulong> Boards { get; } = new();
}