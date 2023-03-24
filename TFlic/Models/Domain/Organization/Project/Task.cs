using TFlic.Models.Domain.Organization.Accounts;

namespace TFlic.Models.Domain.Organization.Project;

public class Task
{
    /// <summary>
    /// Уникальный идентификатор задачи
    /// </summary>
    public ulong Id { get; init; }
    
    public ulong ColumnId { get; init; }
    
    public Column? Column { get; set; }

    /// <summary>
    /// Позиция задачи в столбце
    /// </summary>
    public int Position { get; set; }
    
    /// <summary>
    /// Название задачи
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание задачи
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Авторы задачи
    /// </summary>
    public ICollection<Account> Authors { get; init; } = new List<Account>();
    
    /// <summary>
    /// Время создания задачи
    /// </summary>
    public DateTime CreationTime { get; set; }
    
    /// <summary>
    /// Текущий статус задачи
    /// </summary>
    public string Status { get; set; } = string.Empty;

    public uint Priority { get; set; } = 1;
    
    public ulong ExecutorId { get; init; }

    public TimeSpan? EstimatedTime { get; set; }
    
    public DateTime Deadline { get; set; }
}