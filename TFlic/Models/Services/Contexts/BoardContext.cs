using Microsoft.EntityFrameworkCore;
using TFlic.Models.Domain.Organization.Project;

namespace TFlic.Models.Services.Contexts;

public class BoardContext: DbContext
{
    public DbSet<Board> Boards { get; set; } = null!;
    
    public BoardContext(DbContextOptions<BoardContext> options) : base(options) { }

}