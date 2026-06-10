using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using WorkoutRag.DTO;
using WorkoutRag.Models;
using WorkoutRag.Repositories;

namespace WorkoutRag.Services;

public class WorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUserRepository _userRepository;
    private readonly WorkoutRetrievalService _retrievalService;
    private readonly OllamaService _ollamaService;

    public WorkoutService(
        IWorkoutRepository workoutRepository,
        IUserRepository userRepository,
        WorkoutRetrievalService retrievalService,
        OllamaService ollamaService
    )
    {
        _workoutRepository = workoutRepository;
        _userRepository = userRepository;
        _retrievalService = retrievalService;
        _ollamaService = ollamaService;
    }

    public async Task<string> GenerateAndSaveWorkoutAsync(Guid userId, WorkoutRequest request)
    {
        // 1. Fetch user
        var user = await _userRepository.GetByIdWithProfileAsync(userId);
        if (user == null)
            throw new Exception("User account not found.");

        // 2. Search exercises
        var exercises = await _retrievalService.SearchExercisesAsync(
            request.Prompt,
            request.Equipment
        );

        // 3. Generate workout
        var workoutJson = await _ollamaService.GenerateWorkoutPlanAsync(
            request.Prompt,
            request.Equipment,
            exercises,
            user
        );

        // 4. Save to database
        await SaveWorkoutAsync(userId, request.Prompt, request.Equipment, workoutJson);

        return workoutJson;
    }

    public async Task<WorkoutHistory> SaveWorkoutAsync(
        Guid userId,
        string userPrompt,
        string equipmentFilter,
        string workoutJson
    )
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User account not found.");

        var historyRecord = new WorkoutHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserPrompt = userPrompt,
            EquipmentFilter = equipmentFilter,
            RawAiJson = workoutJson,
            CreatedAt = DateTime.UtcNow,
        };

        await _workoutRepository.AddAsync(historyRecord);
        await _workoutRepository.SaveChangesAsync();

        return historyRecord;
    }

    public async Task<List<WorkoutHistory>> GetUserWorkoutHistoryAsync(Guid userId)
    {
        return await _workoutRepository.GetByUserIdAsync(userId);
    }
}
