using Microsoft.EntityFrameworkCore;
using TFlic.Models.Organization.Project.Component;

namespace TFlic.Models.Contexts;

public class ComponentContext : DbContext
{
    public DbSet<ComponentDto> Components { get; set; } = null!;
    
    public ComponentContext(DbContextOptions<ComponentContext> options) : base(options) { }
}