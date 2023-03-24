using Microsoft.EntityFrameworkCore;
using TFlic.Models.Domain.Organization.Project;

namespace TFlic.Models.Services.Contexts;

public class ColumnContext: DbContext
{
    public DbSet<Column> Columns { get; set; } = null!;
    
    public ColumnContext(DbContextOptions<ColumnContext> options) : base(options) { }

}