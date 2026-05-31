using Microsoft.EntityFrameworkCore;
using WorkoutRag.Data;
using WorkoutRag.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector()
    )
);

// 2. Add Services
builder.Services.AddHttpClient<WorkoutRetrievalService>();
builder.Services.AddScoped<WorkoutRetrievalService>();
builder.Services.AddScoped<OllamaService>();

// 3. Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 4. Migrate Database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    // 3. RUN THE SEEDER ON STARTUP
    await DatabaseSeeder.SeedAsync(app.Services);
}

app.MapControllers();
app.Run();
