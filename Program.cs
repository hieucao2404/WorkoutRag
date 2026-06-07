using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkoutRag.Data;
using WorkoutRag.Repositories;
using WorkoutRag.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector()
    )
);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
    );
});

// 3. Add Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 2. Add Services
builder.Services.AddHttpClient<WorkoutRetrievalService>();
builder.Services.AddScoped<WorkoutRetrievalService>();
builder.Services.AddScoped<OllamaService>();
builder.Services.AddHttpClient<OllamaService>();
builder.Services.AddScoped<UserService>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
        };
    });

// 3. Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

//Use CORS
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

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
