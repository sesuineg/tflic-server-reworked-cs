using Microsoft.EntityFrameworkCore;
using TFlic.Models.Organization.Project;

namespace TFlic.Models.Contexts;

public class ColumnContext: DbContext
{
    public DbSet<Column> Columns { get; set; } = null!;
    
    public ColumnContext(DbContextOptions<ColumnContext> options) : base(options) { }

}