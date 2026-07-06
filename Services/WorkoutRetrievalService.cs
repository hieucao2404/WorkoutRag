using Pgvector;
using WorkoutRag.Data;
using WorkoutRag.Interfaces;
using WorkoutRag.Models;
using WorkoutRag.Repositories.Interfaces;

namespace WorkoutRag.Services;

public class WorkoutRetrievalService : IWorkoutRetrievalService
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IOllamaService _ollamaService;

    public WorkoutRetrievalService(
        IExerciseRepository exerciseRepository,
        IOllamaService ollamaService
    )
    {
        _exerciseRepository = exerciseRepository;
        _ollamaService = ollamaService;
    }

    public async Task<List<Exercise>> SearchExercisesAsync(
        string userPrompt,
        string equipmentFilter,
        int limit = 6
    )
    {
        var userVectorArray = await _ollamaService.GetEmbeddingAsync(userPrompt);
        var userVector = new Vector(userVectorArray);

        return await _exerciseRepository.SearchByVectorAsync(userVector, equipmentFilter, limit);
    }
}
