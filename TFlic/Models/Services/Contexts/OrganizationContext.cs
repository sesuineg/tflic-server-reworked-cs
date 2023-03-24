using Microsoft.EntityFrameworkCore;
using TFlic.Models.Domain.Organization;

namespace TFlic.Models.Services.Contexts;

public class OrganizationContext : DbContext
{
    public DbSet<Organization> Organizations { get; set; } = null!;
    
    public OrganizationContext(DbContextOptions<OrganizationContext> options) : base(options) { }
}