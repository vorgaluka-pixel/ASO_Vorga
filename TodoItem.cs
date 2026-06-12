namespace TodoApp.Models;

public class TodoItem
{
    public int Id { get; set; }

    public string Titlu { get; set; } = string.Empty;

    public string? Descriere { get; set; }

    public bool EsteCompletat { get; set; } = false;

    public DateTime DataCreare { get; set; } = DateTime.UtcNow;

    public DateTime? DataFinalizare { get; set; }

    public Priority Prioritate { get; set; } = Priority.Normal;
}

public enum Priority
{
    Low = 0,
    Normal = 1,
    High = 2
}
