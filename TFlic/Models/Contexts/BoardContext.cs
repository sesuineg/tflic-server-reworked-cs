using Microsoft.EntityFrameworkCore;
using TFlic.Models.Organization.Project;

namespace TFlic.Models.Contexts;

public class BoardContext: DbContext
{
    public DbSet<Board> Boards { get; set; } = null!;
    
    public BoardContext(DbContextOptions<BoardContext> options) : base(options) { }

}