using Microsoft.EntityFrameworkCore;
using Task = Server.Models.Organization.Project.Task;

namespace Server.Models.Contexts;

public class TaskContext: DbContext
{
    public DbSet<Organization.Project.Task> Tasks { get; set; } = null!;
    
    public TaskContext(DbContextOptions<TaskContext> options) : base(options) { }

}