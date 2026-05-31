using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using WorkoutRag.Data;
using WorkoutRag.Models;

namespace WorkoutRag.Services;

public class WorkoutRetrievalService
{
    private readonly AppDbContext _context;
    private readonly OllamaService _ollamaService;

    public WorkoutRetrievalService(AppDbContext context, OllamaService ollamaService)
    {
        _context = context;
        _ollamaService = ollamaService;
    }

    public async Task<List<Exercise>> SearchExercisesAsync(
        string userPrompt,
        string equipmentFilter,
        int limit = 3
    )
    {
        // 1. Turn the user's plain text prompt into a math vector
        var userVectorArray = await _ollamaService.GetEmbeddingAsync(userPrompt);
        var userVector = new Vector(userVectorArray);

        // 2. Perform the Hybrid Search in PostgreSQL
        var recommendedExercises = await _context
            .Exercises
            // HYBRID STEP A: Strict SQL Filter (Only show exercises they have equipment for)
            .Where(e => e.Equipment.ToLower() == equipmentFilter.ToLower())
            // HYBRID STEP B: Vector Math (Order by how closely the AI thinks it matches their prompt)
            .OrderBy(e => e.Embedding!.CosineDistance(userVector))
            // Limit to top results
            .Take(limit)
            .ToListAsync();

        return recommendedExercises;
    }
}
