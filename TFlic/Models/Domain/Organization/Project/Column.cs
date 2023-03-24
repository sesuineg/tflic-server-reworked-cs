namespace TFlic.Models.Domain.Organization.Project;

public class Column
{
    /// <summary>
    /// Уникальный идентификатор столбца
    /// </summary>
    public ulong Id { get; set; }
    
    public ulong BoardId { get; set; }

    public Board? Board { get; set; }

    /// <summary>
    /// Название столбца
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Позиция столбца на доске
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Задачи
    /// </summary>
    public ICollection<Task> Tasks { get; init; } = new List<Task>();
    
    /// <summary>
    /// Переместить задачу в другую позицию
    /// </summary>
    /// <param name="id">id задачи</param>
    /// <param name="position">В какую позицию переложить задачу</param>
    /// <returns>Возвращает false, если не задачи с таким id нет или кол-во задач <= выбранной позиции</returns>
    public bool MoveTask(ulong id, int position)
    {
        if (Tasks.Count <= position || !ContainTask(id))
            return false;
        var targetTask = GetTask(id);
        targetTask!.Position = position; // восклицательный знак только потому, что метод не используется и будет переписан
        foreach (var item in Tasks.Where(task => task.Position >= targetTask.Position))
            item.Position--;
        return true;
    }
    
    /// <summary>
    /// Проверка на наличие задачи с выбранным id
    /// </summary>
    /// <param name="id">id задачи</param>
    /// <returns>Возвращает true. если есть задача с выбранным id</returns>
    public bool ContainTask(ulong id)
    {
        return Tasks.Any(task => task.Id == id);
    }
    /// <summary>
    /// Возвращает задачу по id
    /// </summary>
    /// <param name="id">id задачи</param>
    /// <returns>Объект Task или null</returns>
    public Task? GetTask(ulong id)
    {
        return ContainTask(id) ? Tasks.Single(task => task.Id == id) : null;
    }

    /// <summary>
    /// Добавляет задачу в колонку 
    /// </summary>
    /// <param name="targetTask">Задача, которую желаем добавить в колонку</param>
    /// <returns>Возвращает false, если уже существует задача с таким id</returns>
    public bool AddTask(Task targetTask)
    {
        if(ContainTask(targetTask.Id))
            return false;
        targetTask.Position = Tasks.Max(task => task.Position) + 1;
        Tasks.Add(targetTask);
        return true;
    }
}