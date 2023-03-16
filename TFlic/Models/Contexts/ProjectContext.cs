using Microsoft.EntityFrameworkCore;
using TFlic.Models.Organization.Project;

namespace TFlic.Models.Contexts;

public class ProjectContext: DbContext
{
    public DbSet<Project> Projects { get; set; } = null!;

    public ProjectContext(DbContextOptions<ProjectContext> options) : base(options) { }
}