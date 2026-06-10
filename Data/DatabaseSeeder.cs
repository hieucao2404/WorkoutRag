using System.Text.Json;
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

        // If data exists, bail out early to keep boot fast
        if (await context.Exercises.AnyAsync())
            return;

        // Resolve path to our dedicated SeedData JSON file
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var jsonFilePath = Path.Combine(baseDirectory, "Data", "exercises.json");

        // Fallback fallback path adjustment for development runtimes (dotnet watch)
        if (!File.Exists(jsonFilePath))
        {
            jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "exercises.json");
        }

        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"[ERROR] Seed data blueprint missing at: {jsonFilePath}");
            return;
        }

        Console.WriteLine("Reading bulk exercise data from JSON library...");
        var rawJson = await File.ReadAllTextAsync(jsonFilePath);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var exercisesToSeed = JsonSerializer.Deserialize<List<Exercise>>(rawJson, options);

        if (exercisesToSeed == null || !exercisesToSeed.Any())
        {
            Console.WriteLine("Seed JSON library parsed empty. Skipping execution.");
            return;
        }

        Console.WriteLine(
            $"Synchronizing {exercisesToSeed.Count} athletic movements with local AI Engine..."
        );

        int processedIndex = 1;
        foreach (var exercise in exercisesToSeed)
        {
            Console.WriteLine(
                $"[{processedIndex}/{exercisesToSeed.Count}] Embedding multi-sport semantic vectors: {exercise.Name}"
            );

            try
            {
                // Concat Name and Description for enhanced vector accuracy
                var semanticInput = $"{exercise.Name}: {exercise.Description}";
                var vectorArray = await ollamaService.GetEmbeddingAsync(semanticInput);

                exercise.Embedding = new Vector(vectorArray);
                context.Exercises.Add(exercise);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[CRITICAL] Embedding generation failed for '{exercise.Name}': {ex.Message}"
                );
                // Continue to next exercise so one failure doesn't ruin the entire batch
                continue;
            }

            processedIndex++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Successfully initialized sport-specific vector database layout!");
    }
}
