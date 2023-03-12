using Microsoft.EntityFrameworkCore;
using Server.Models.Organization.Project;

namespace Server.Models.Contexts;

public class ProjectContext: DbContext
{
    public DbSet<Project> Projects { get; set; } = null!;

    public ProjectContext(DbContextOptions<ProjectContext> options) : base(options) { }
}