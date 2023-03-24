using Microsoft.EntityFrameworkCore;
using TFlic.Models.Domain.Organization.Project;

namespace TFlic.Models.Services.Contexts;

public class ProjectContext: DbContext
{
    public DbSet<Project> Projects { get; set; } = null!;

    public ProjectContext(DbContextOptions<ProjectContext> options) : base(options) { }
}