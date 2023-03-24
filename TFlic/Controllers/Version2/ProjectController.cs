using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TFlic.Controllers.Version2.DTO.GET;
using TFlic.Controllers.Version2.DTO.POST;
using TFlic.Models.Domain.Organization.Project;
using TFlic.Models.Services.Contexts;

namespace TFlic.Controllers.Version2;

[Authorize]
[ApiController]
[Route("api/v2")]
public class ProjectController : ControllerBase
{
    public ProjectController(TFlicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("organizations/{organizationId}/projects")]
    public ActionResult<IEnumerable<ProjectGet>> GetProjects(ulong organizationId)
    {
        var organization = _dbContext.Organizations
            .SingleOrDefault(org => org.Id == organizationId);
        if (organization is null)
            return NotFound();

        var projects = _dbContext.Projects
            .Where(project => project.OrganizationId == organizationId)
            .Select(project => new ProjectGet(project));

        return Ok(projects);
    }
    
    [HttpGet("projects/{projectId}")]
    public ActionResult<ProjectGet> GetProject(ulong projectId)
    {
        var project = _dbContext.Projects.SingleOrDefault(project => project.Id == projectId);
        return project is not null
            ? new ProjectGet(project)
            : NotFound();
    }
    

    [HttpDelete("projects/{projectId}")]
    public ActionResult DeleteProjects(ulong projectId)
    {
        var projectToDelete = _dbContext.Projects.SingleOrDefault(project => project.Id == projectId);
        if (projectToDelete is null)
            return NotFound();

        _dbContext.Projects.Remove(projectToDelete);
        _dbContext.SaveChanges();

        return Ok();
    }

    [HttpPost("organizations/{organizationId}/projects")]
    public ActionResult<ProjectGet> CreateProject(ulong organizationId, ProjectDto project)
    {
        var organization = _dbContext.Organizations
            .SingleOrDefault(org => org.Id == organizationId);
        if (organization is null)
            return NotFound();

        var newProject = new Project
        {
            Name = project.Name,
            OrganizationId = organizationId
        };
        _dbContext.Projects.Add(newProject);
        _dbContext.SaveChanges();
            
        return Ok(new ProjectGet(newProject));
    }

    [HttpPatch("projects/{projectId}")]
    public ActionResult<ProjectGet> PatchProject(ulong projectId, [FromBody] JsonPatchDocument<Project> patch)
    {
        var projectToPatch = _dbContext.Projects.SingleOrDefault(project => project.Id == projectId);
        if (projectToPatch is null)
            return NotFound();
        
        patch.ApplyTo(projectToPatch);
        _dbContext.SaveChanges();
        
        return Ok(new ProjectGet(projectToPatch));
    }
    
    
    
    private readonly TFlicDbContext _dbContext;
}
