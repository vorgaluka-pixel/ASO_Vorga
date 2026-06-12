using Microsoft.EntityFrameworkCore;
using TodoApp.Data;

var constructorAplicatie = WebApplication.CreateBuilder(args);

// ── Servicii ────────────────────────────────────────────────────────────────
constructorAplicatie.Services.AddControllers();
constructorAplicatie.Services.AddEndpointsApiExplorer();
constructorAplicatie.Services.AddSwaggerGen(cfg =>
{
    cfg.SwaggerDoc("v1", new() { Title = "TodoApp API", Version = "v1" });
});

// Baza de date SQLite (fișier local todo.db)
constructorAplicatie.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(
        constructorAplicatie.Configuration.GetConnectionString("Default")
        ?? "Data Source=todo.db"
    ));

var aplicatie = constructorAplicatie.Build();

// ── Migrare automată la pornire ──────────────────────────────────────────────
using (var scope = aplicatie.Services.CreateScope())
{
    var bazaDate = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    bazaDate.Database.EnsureCreated();
}

// ── Middleware ───────────────────────────────────────────────────────────────
if (aplicatie.Environment.IsDevelopment())
{
    aplicatie.UseSwagger();
    aplicatie.UseSwaggerUI();
}

aplicatie.UseDefaultFiles();
aplicatie.UseStaticFiles();

aplicatie.UseRouting();
aplicatie.MapControllers();

aplicatie.Run();
