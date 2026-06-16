using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore; // Added for CosineDistance LINQ translation
using WorkoutRag.Data;
using WorkoutRag.Models;

namespace WorkoutRag.Repositories;

// 1. Fixed typo in class name: ExerciseRepository
public class ExerciseRepository : Repository<Exercise>, IExerciseRepository
{
    // 2. Fixed typo here: AppDbContext
    private readonly AppDbContext _context;

    public ExerciseRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    // Hybrid vector search with optimal equipment filter
    public async Task<List<Exercise>> SearchByVectorAsync(
        Vector userVector, // 3. Fixed parameter name: userVector
        string? equipmentFilter = null,
        int limit = 3
    )
    {
        var query = _context.Exercises.AsQueryable();

        if (!string.IsNullOrWhiteSpace(equipmentFilter))
        {
            query = query.Where(e => e.Equipment.ToLower() == equipmentFilter.ToLower());
        }

        var results = await query
            .OrderBy(e => e.Embedding.CosineDistance(userVector))
            .Take(limit)
            .ToListAsync();

        return results;
    }

    // 4. Fixed method name to match interface: GetByDifficultyAsync
    public async Task<List<Exercise>> GetByDifficultyAsync(string difficulty)
    {
        return await _context.Exercises.Where(e => e.DifficultyLevel == difficulty).ToListAsync();
    }

    public async Task<List<Exercise>> GetByMuscleAsync(string muscle)
    {
        return await _context
            .Exercises.Where(e => e.MusclesTargeted.Contains(muscle))
            .ToListAsync();
    }

    public async Task<List<Exercise>> GetByMovementPatternAsync(string pattern)
    {
        return await _context.Exercises.Where(e => e.MovementPattern == pattern).ToListAsync();
    }
}
