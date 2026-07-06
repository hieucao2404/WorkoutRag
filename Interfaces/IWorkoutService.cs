using WorkoutRag.DTO;
using WorkoutRag.Models;

namespace WorkoutRag.Interfaces;

public interface IWorkoutService
{
    Task<string> GenerateWorkoutAsync(Guid userId, WorkoutRequest request);
    Task<WorkoutHistory> SaveWorkoutAsync(Guid userId, string userPrompt, string equipmentFilter, string workoutJson);
    Task<List<WorkoutHistory>> GetUserWorkoutHistoryAsync(Guid userId);
}