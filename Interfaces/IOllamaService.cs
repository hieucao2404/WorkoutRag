using WorkoutRag.Models;

namespace WorkoutRag.Interfaces;

public interface IOllamaService
{
    Task<float[]> GetEmbeddingAsync(string text);
    Task<string> GenerateWorkoutPlanAsync(string userGoal, string equipment, List<Exercise> exercises, User user);
    Task<string> GenerateNutritionPlanAsync(string goal, User user, UserDiet userDiet);

    Task<string> SendPromptAsync(string prompt);
}