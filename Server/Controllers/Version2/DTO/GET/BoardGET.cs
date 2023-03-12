namespace Server.Controllers.Version2.DTO.GET;

public class BoardGet
{
    public BoardGet(Models.Organization.Project.Board board)
    {
        Id = board.id;
        Name = board.Name;
        if (board.Columns == null) return;
        foreach (var column in board.Columns) { Columns.Add(column.Id); }
    }
    
    public ulong Id { get; set; }
    public string Name { get; set; }
    public List<ulong> Columns { get; } = new();
}