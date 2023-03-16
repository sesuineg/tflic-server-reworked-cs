using TFlic.Models.Contexts;
using TFlic.Models.Organization.Project;

namespace TFlic.Controllers.Version2.Service;

// todo избавиться от PathChecker
public static class PathChecker
{

    public static bool IsTaskPathCorrect(IEnumerable<Models.Organization.Project.Task> task, ulong organizationId,
        ulong projectId, ulong boardId, ulong columnId)
    {
        return task.Any(x => x.ColumnId == columnId
                             && x.Column.BoardId == boardId
                             && x.Column.Board.ProjectId == projectId
                             && x.Column.Board.Project.OrganizationId == organizationId);
    }
    
    public static bool IsColumnPathCorrect(IEnumerable<Column> columns, ulong organizationId,
        ulong projectId, ulong boardId)
    {
        return columns.Any(x => x.BoardId == boardId
                             && x.Board.ProjectId == projectId
                             && x.Board.Project.OrganizationId == organizationId);
    }
    
    public static bool IsBoardPathCorrect(IEnumerable<Board> boards, ulong organizationId,
        ulong projectId)
    {
        return boards.Any(x => x.ProjectId == projectId
                                && x.Project.OrganizationId == organizationId);
    }
    
    public static bool IsProjectPathCorrect(IEnumerable<Project> projects, ulong organizationId)
    {
        return projects.Any(x => x.OrganizationId == organizationId);
    }
    
    /*/////////////////////////////*/
    
    public static bool IsComponentPathCorrect(ulong organizationId, ulong projectId, 
        ulong boardId, ulong columnId, ulong taskId)
    {
        using var pathCtx = DbContexts.Get<TaskContext>();
        var prj = ContextIncluder.GetTask(pathCtx).Where(x => x.Id == taskId);
        return IsTaskPathCorrect(prj, organizationId, projectId, boardId, columnId);
    }
    
    public static bool IsTaskPathCorrect(ulong organizationId,
        ulong projectId, ulong boardId, ulong columnId)
    {
        using var pathCtx = DbContexts.Get<ColumnContext>();
        var prj = ContextIncluder.GetColumn(pathCtx).Where(x => x.Id == columnId);
        return IsColumnPathCorrect(prj, organizationId, projectId, boardId);
    }
    
    public static bool IsColumnPathCorrect(ulong organizationId,
        ulong projectId, ulong boardId)
    {
        using var pathCtx = DbContexts.Get<BoardContext>();
        var obj = ContextIncluder.GetBoard(pathCtx).Where(x => x.id == boardId);
        return IsBoardPathCorrect(obj, organizationId, projectId);
    }
    
    public static bool IsBoardPathCorrect(ulong organizationId,
        ulong projectId)
    {
        using var pathCtx = DbContexts.Get<ProjectContext>();
        var obj = ContextIncluder.GetProject(pathCtx).Where(x => x.id == projectId);
        return IsProjectPathCorrect(obj, organizationId);
    }
    
    public static bool IsProjectPathCorrect(ulong organizationId)
    {
        using var pathCtx = DbContexts.Get<OrganizationContext>();
        return pathCtx.Organizations.Any(x => x.Id == organizationId);
    }
}