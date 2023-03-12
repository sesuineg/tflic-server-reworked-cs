using Microsoft.EntityFrameworkCore;
using Server.Models.Organization.Project;

namespace Server.Models.Contexts;

public class BoardContext: DbContext
{
    public DbSet<Board> Boards { get; set; } = null!;
    
    public BoardContext(DbContextOptions<BoardContext> options) : base(options) { }

}