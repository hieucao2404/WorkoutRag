using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pgvector;
using WorkoutRag.Models;
using WorkoutRag.Services;

namespace WorkoutRag.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ollamaService = scope.ServiceProvider.GetRequiredService<OllamaService>();

        await context.Database.MigrateAsync();

        //If we already have exercises, skip seeding
        if (await context.Exercises.AnyAsync())
            return;

        Console.WriteLine(
            "Database is empty. Generating AI vectors for Seed Data ... This will take a moment"
        );

        var exercisesToSeed = new List<Exercise>
        {
            new Exercise
            {
                Name = "Push-Up",
                Description =
                    "A classic bodyweight movement targeting the chest, shoulders, and triceps. Excellent for beginners building upper body strength.",
                Equipment = "Bodyweight",
                DifficultyLevel = "Beginner",
                MovementPattern = "Push",
                ExerciseType = "Compound",
                MusclesTargeted = new List<string> { "Chest", "Triceps", "Front Delts" },
            },
            new Exercise
            {
                Name = "Box Jump",
                Description =
                    "A plyometric exercise focused on lower-body explosiveness and fast-twitch muscle fibers. Highly recommended for sports requiring vertical leaps, such as basketball.",
                Equipment = "Box",
                DifficultyLevel = "Intermediate",
                MovementPattern = "Squat",
                ExerciseType = "Plyometric",
                MusclesTargeted = new List<string> { "Quads", "Glutes", "Calves" },
            },
        };

        // Loop through each exercise, ask Ollama for the math, and save it
        foreach (var exercise in exercisesToSeed)
        {
            var vectorArray = await ollamaService.GetEmbeddingAsync(exercise.Description);
            exercise.Embedding = new Vector(vectorArray);
            context.Exercises.Add(exercise);
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Seed data and vectors successfully saved to PostgreSQL!");
    }
}
