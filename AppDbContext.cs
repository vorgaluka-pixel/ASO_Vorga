using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> optiuni) 
        : base(optiuni) { }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entitate =>
        {
            entitate.HasKey(e => e.Id);

            entitate.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entitate.Property(e => e.Description)
                .HasMaxLength(1000);
        });
    }
}
