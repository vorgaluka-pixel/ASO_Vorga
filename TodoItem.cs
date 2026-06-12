namespace TodoApp.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public Priority Priority { get; set; } = Priority.Normal;
}

public enum Priority
{
    Low = 0,
    Normal = 1,
    High = 2
}
