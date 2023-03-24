using Microsoft.EntityFrameworkCore;
using Task = TFlic.Models.Domain.Organization.Project.Task;

namespace TFlic.Models.Services.Contexts;

public class TaskContext: DbContext
{
    public DbSet<Task> Tasks { get; set; } = null!;
    
    public TaskContext(DbContextOptions<TaskContext> options) : base(options) { }

}