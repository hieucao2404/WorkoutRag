using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pgvector;
using WorkoutRag.Models;

namespace WorkoutRag.Repositories;

public interface IExerciseRepository : IRepository<Exercise>
{
    //Custom method for exercise
    Task<List<Exercise>> SearchByVectorAsync(
        Vector userVector,
        string? equipmentFilter = null,
        int limit = 3
    );
    Task<List<Exercise>> GetByDifficultyAsync(string difficulty);
    Task<List<Exercise>> GetByMuscleAsync(string muscle);
    Task<List<Exercise>> GetByMovementPatternAsync(string pattern);
}
