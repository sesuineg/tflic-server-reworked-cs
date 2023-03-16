using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TFlic.Models.Contexts;
using TFlic.Models.Organization;
using TFlic.Models.Organization.Project;
using TFlic.Models.Organization.Project.Component;

namespace TFlic.Controllers.Version2.Service;

// todo избавиться от ContextIncluder
public static class ContextIncluder
{
    public static IIncludableQueryable<ComponentDto, Organization> GetComponent(ComponentContext ctx)
    {
        return ctx.Components
            .Include(x => x.Task)
            .ThenInclude(x => x.Column)
            .ThenInclude(x => x.Board)
            .ThenInclude(x => x.Project)
            .ThenInclude(x => x.Organization);
    }

    public static IIncludableQueryable<Models.Organization.Project.Task, Organization> GetTask(TaskContext ctx)
    {
        return ctx.Tasks
            .Include(x => x.Column)
            .ThenInclude(x => x.Board)
            .ThenInclude(x => x.Project)
            .ThenInclude(x => x.Organization);
    }

    public static IIncludableQueryable<Column, Organization> GetColumn(ColumnContext ctx)
    {
        return ctx.Columns
            .Include(x => x.Board)
            .ThenInclude(x => x.Project)
            .ThenInclude(x => x.Organization);
    }

    public static IIncludableQueryable<Board, Organization> GetBoard(BoardContext ctx)
    {
        return ctx.Boards
            .Include(x => x.Project)
            .ThenInclude(x => x.Organization);
    }

    public static IIncludableQueryable<Project, Organization> GetProject(ProjectContext ctx)
    {
        return ctx.Projects
            .Include(x => x.Organization);
    }

    public static DbSet<Models.Organization.Project.Task> DeleteTask(TaskContext ctx)
    {
        return ctx.Tasks;
    }

    public static IIncludableQueryable<Column, ICollection<ComponentDto>> DeleteColumn(ColumnContext ctx)
    {
        return ctx.Columns
            .Include(x => x.Tasks)
            .ThenInclude(x => x.Components);
    }

    public static IIncludableQueryable<Board, ICollection<ComponentDto>> DeleteBoard(BoardContext ctx)
    {
        return ctx.Boards
            .Include(x => x.Columns)
            .ThenInclude(x => x.Tasks)
            .ThenInclude(x => x.Components);
    }

    public static IIncludableQueryable<Project, ICollection<ComponentDto>> DeleteProject(ProjectContext ctx)
    {
        return ctx.Projects
            .Include(x => x.boards)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.Tasks)
            .ThenInclude(x => x.Components);
    }
    
    public static DbSet<ComponentDto> DeleteComponent(ComponentContext ctx)
    {
        return ctx.Components;
    }
}
