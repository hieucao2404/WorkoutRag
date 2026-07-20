using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using WorkoutRag.DTO;
using WorkoutRag.Interfaces;
using WorkoutRag.Models;
using WorkoutRag.Repositories.Interfaces;

namespace WorkoutRag.Services;

public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkoutRetrievalService _retrievalService;
    private readonly IOllamaService _ollamaService;
    private readonly IWorkoutGenerator _iworkoutGenerator;

    public WorkoutService(
        IWorkoutRepository workoutRepository,
        IUserRepository userRepository,
        IWorkoutRetrievalService retrievalService,
        IOllamaService ollamaService
    )
    {
        _workoutRepository = workoutRepository;
        _userRepository = userRepository;
        _retrievalService = retrievalService;
        _ollamaService = ollamaService;
    }

    private async Task<WorkoutGenerationContext> BuildGenerationContextAsync(
        Guid userId,
        WorkoutRequest request
    )
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        return new WorkoutGenerationContext
        {
            Age = user.Age ?? 25,
            Height = (float)(user.HeightCm ?? 170m), // Notice the (float) cast and the 'm' for decimal
            Weight = (float)(user.WeightKg ?? 70m), // Notice the (float) cast and the 'm' for decimal
            FitnessLevel = user.AthleticLevel,
            Goal = request.Goal,
            AvailableEquipment = request.AvailableEquipment,
            WorkoutDuration = 45, // Default duration
            // You can map the rest of the fields from your User/Profile model here!

            PreviousWorkoutJson = request.PreviousWorkoutJson,
            UserFeedback = request.UserFeedback,
        };
    }

    public async Task<string> GenerateWorkoutAsync(Guid userId, WorkoutRequest request)
    {
        // 1. Assemble the unified context from the DB and Request
        var context = await BuildGenerationContextAsync(userId, request);
        // 2. Route to the correct pipeline
        IWorkoutGenerator generator = request.UseRag
            ? new RagGenerator(_ollamaService, _retrievalService)
            : new LlmOnlyGenerator(_ollamaService);
        // 3. Generate!
        return await generator.GenerateAsync(context);
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
