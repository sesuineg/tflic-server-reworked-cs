using Microsoft.EntityFrameworkCore;
using Server.Models.Organization.Project;

namespace Server.Models.Contexts;

public class ColumnContext: DbContext
{
    public DbSet<Column> Columns { get; set; } = null!;
    
    public ColumnContext(DbContextOptions<ColumnContext> options) : base(options) { }

}