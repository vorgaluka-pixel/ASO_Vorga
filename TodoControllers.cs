using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly AppDbContext _db;

    public TodosController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/todos
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? completed)
    {
        var query = _db.TodoItems.AsQueryable();

        if (completed.HasValue)
            query = query.Where(t => t.IsCompleted == completed.Value);

        var items = await query
            .AsNoTracking()
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        return Ok(items);
    }

    // GET /api/todos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.TodoItems
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return item is null ? NotFound() : Ok(item);
    }

    // POST /api/todos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var todo = new TodoItem
        {
            Title       = dto.Title,
            Description = dto.Description,
            Priority    = dto.Priority
        };

        _db.TodoItems.Add(todo);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    // PUT /api/todos/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var todo = await _db.TodoItems.FindAsync(id);
        if (todo is null) return NotFound();

        if (dto.Title is not null)
            todo.Title = dto.Title;

        if (dto.Description is not null)
            todo.Description = dto.Description;

        if (dto.Priority.HasValue)
            todo.Priority = dto.Priority.Value;

        if (dto.IsCompleted.HasValue && todo.IsCompleted != dto.IsCompleted.Value)
        {
            todo.IsCompleted = dto.IsCompleted.Value;
            todo.CompletedAt = todo.IsCompleted ? DateTime.UtcNow : null;
        }

        await _db.SaveChangesAsync();
        return Ok(todo);
    }

    // PATCH /api/todos/{id}/toggle
    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var todo = await _db.TodoItems.FindAsync(id);
        if (todo is null) return NotFound();

        todo.IsCompleted  = !todo.IsCompleted;
        todo.CompletedAt  = todo.IsCompleted ? DateTime.UtcNow : null;

        await _db.SaveChangesAsync();
        return Ok(todo);
    }

    // DELETE /api/todos/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await _db.TodoItems.FindAsync(id);
        if (todo is null) return NotFound();

        _db.TodoItems.Remove(todo);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record CreateTodoDto(
    [property: System.ComponentModel.DataAnnotations.Required]
    [property: System.ComponentModel.DataAnnotations.StringLength(200, MinimumLength = 1)]
    string Title,

    [property: System.ComponentModel.DataAnnotations.StringLength(1000)]
    string? Description,

    [property: System.ComponentModel.DataAnnotations.Range(0, 2)]
    Priority Priority = Priority.Normal
);

public record UpdateTodoDto(
    [property: System.ComponentModel.DataAnnotations.StringLength(200, MinimumLength = 1)]
    string? Title,

    [property: System.ComponentModel.DataAnnotations.StringLength(1000)]
    string? Description,

    [property: System.ComponentModel.DataAnnotations.Range(0, 2)]
    Priority? Priority,

    bool? IsCompleted
);
