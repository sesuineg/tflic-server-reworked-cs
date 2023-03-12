using Microsoft.EntityFrameworkCore;
using Server.Models.Organization.Project.Component;

namespace Server.Models.Contexts;

public class ComponentContext : DbContext
{
    public DbSet<ComponentDto> Components { get; set; } = null!;
    
    public ComponentContext(DbContextOptions<ComponentContext> options) : base(options) { }
}