using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodosController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/todos
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? esteFinalizat)
    {
        var interogare = _context.TodoItems.AsQueryable();

        if (esteFinalizat.HasValue)
            interogare = interogare.Where(t => t.IsCompleted == esteFinalizat.Value);

        var lista = await interogare
            .AsNoTracking()
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        return Ok(lista);
    }

    // GET /api/todos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var element = await _context.TodoItems
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return element is null ? NotFound() : Ok(element);
    }

    // POST /api/todos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoDto date)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var sarcina = new TodoItem
        {
            Title = date.Title,
            Description = date.Description,
            Priority = date.Priority
        };

        _context.TodoItems.Add(sarcina);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = sarcina.Id }, sarcina);
    }

    // PUT /api/todos/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoDto date)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var sarcina = await _context.TodoItems.FindAsync(id);
        if (sarcina is null) return NotFound();

        if (date.Title is not null)
            sarcina.Title = date.Title;

        if (date.Description is not null)
            sarcina.Description = date.Description;

        if (date.Priority.HasValue)
            sarcina.Priority = date.Priority.Value;

        if (date.IsCompleted.HasValue && sarcina.IsCompleted != date.IsCompleted.Value)
        {
            sarcina.IsCompleted = date.IsCompleted.Value;
            sarcina.CompletedAt = sarcina.IsCompleted ? DateTime.UtcNow : null;
        }

        await _context.SaveChangesAsync();
        return Ok(sarcina);
    }

    // PATCH /api/todos/{id}/toggle
    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var sarcina = await _context.TodoItems.FindAsync(id);
        if (sarcina is null) return NotFound();

        sarcina.IsCompleted = !sarcina.IsCompleted;
        sarcina.CompletedAt = sarcina.IsCompleted ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();
        return Ok(sarcina);
    }

    // DELETE /api/todos/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var sarcina = await _context.TodoItems.FindAsync(id);
        if (sarcina is null) return NotFound();

        _context.TodoItems.Remove(sarcina);
        await _context.SaveChangesAsync();
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
