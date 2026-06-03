using Pgvector;
using WorkoutRag.Data;
using WorkoutRag.Models;
using WorkoutRag.Repositories;

namespace WorkoutRag.Services;

public class WorkoutRetrievalService
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly OllamaService _ollamaService;

    public WorkoutRetrievalService(
        IExerciseRepository exerciseRepository,
        OllamaService ollamaService
    )
    {
        _exerciseRepository = exerciseRepository;
        _ollamaService = ollamaService;
    }

    public async Task<List<Exercise>> SearchExercisesAsync(
        string userPrompt,
        string equipmentFilter,
        int limit = 3
    )
    {
        var userVectorArray = await _ollamaService.GetEmbeddingAsync(userPrompt);
        var userVector = new Vector(userVectorArray);

        return await _exerciseRepository.SearchByVectorAsync(userVector, equipmentFilter, limit);
    }
}
