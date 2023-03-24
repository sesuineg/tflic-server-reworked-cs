using System.ComponentModel.DataAnnotations;

namespace TFlic.Models.Domain.Organization.Project;

public class Board
{
    /// <summary>
    /// Уникальный идентификатор доски
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// Название доски
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public ulong ProjectId { get; set; }

    public Project? Project { get; set; }

    /// <summary>
    /// Столбцы доски
    /// </summary>
    public ICollection<Column> Columns { get; init; } = new List<Column>();
}
