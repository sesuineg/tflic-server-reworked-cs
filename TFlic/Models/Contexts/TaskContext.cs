using Microsoft.EntityFrameworkCore;
using Task = TFlic.Models.Organization.Project.Task;

namespace TFlic.Models.Contexts;

public class TaskContext: DbContext
{
    public DbSet<Organization.Project.Task> Tasks { get; set; } = null!;
    
    public TaskContext(DbContextOptions<TaskContext> options) : base(options) { }

}