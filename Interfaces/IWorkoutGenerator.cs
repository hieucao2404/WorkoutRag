using WorkoutRag.DTO;

namespace WorkoutRag.Interfaces;

public interface IWorkoutGenerator{
    Task<string> GenerateAsync(WorkoutGenerationContext context);
}