#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Embeddings;
using WorkoutRag.Models;

namespace WorkoutRag.Services;

public class WorkoutRetrievalService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;

    // Mock exercise database for now
    private readonly List<ExerciseRecord> _exercises = new()
    {
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Dumbbell Bench Press",
            Description = "A compound chest push using dumbbells.",
            Equipment = "dumbbells",
            TargetMuscles = new List<string> { "chest", "triceps" },
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Dumbbell Chest Fly",
            Description = "An isolation fly movement for lower chest stretch.",
            Equipment = "dumbbells",
            TargetMuscles = new List<string> { "chest" },
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Barbell Squat",
            Description = "Heavy lower body compound movement focusing on quads.",
            Equipment = "barbell",
            TargetMuscles = new List<string> { "quads", "glutes" },
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Push-Up",
            Description = "Bodyweight chest and tricep pressing progression.",
            Equipment = "bodyweight",
            TargetMuscles = new List<string> { "chest", "triceps" },
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Lat Pulldown",
            Description = "An isolation lats movement for wider back.",
            Equipment = "machine",
            TargetMuscles = new List<string> { "back", "biceps" },
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Pull-up",
            Description = "Bodyweight back and bicep pulling progression.",
            Equipment = "bodyweight",
            TargetMuscles = new List<string> { "back", "biceps" },
        },
    };

    public WorkoutRetrievalService(
        ITextEmbeddingGenerationService embeddingService,
        string connectionString
    )
    {
        _embeddingService = embeddingService;
        // connectionString parameter kept for future database integration
    }

    public async Task InitializeAndSeedAsync()
    {
        Console.WriteLine("-> Checking database schema and seeding...");

        // Mock initialization - in production, this would connect to PostgreSQL with pgvector
        foreach (var ex in _exercises)
        {
            string descriptiveChunk =
                $"{ex.Name}: {ex.Description} targeting {string.Join(", ", ex.TargetMuscles)} with {ex.Equipment}.";
            try
            {
                ex.Embedding = await _embeddingService.GenerateEmbeddingAsync(descriptiveChunk);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: Could not generate embedding: {e.Message}");
            }
        }

        Console.WriteLine("-> Database loaded successfully");
    }

    public async Task<string> SearchExercisesAsync(string userGoal, string availableEquipment)
    {
        try
        {
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(userGoal);

            // Filter exercises by available equipment
            var matchingExercises = _exercises.FindAll(e => e.Equipment == availableEquipment);

            var context = new List<object>();
            foreach (var ex in matchingExercises)
            {
                context.Add(
                    new
                    {
                        ex.Name,
                        ex.Description,
                        ex.Equipment,
                        ex.TargetMuscles,
                    }
                );
            }

            return JsonSerializer.Serialize(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Search error: {ex.Message}");
            return JsonSerializer.Serialize(new List<object>());
        }
    }
}
