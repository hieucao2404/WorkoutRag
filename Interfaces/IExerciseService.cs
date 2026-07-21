using WorkoutRag.DTO;

namespace WorkoutRag.Interfaces;

public interface IExerciseService
{
    Task<List<AdminExerciseResponse>> GetExercisesAsync();
    Task<AdminExerciseResponse> CreateExerciseAsync(AdminExerciseRequest request);
    Task<AdminExerciseResponse> UpdateExerciseAsync(Guid id, AdminExerciseRequest request);
    Task DeleteExerciseAsync(Guid id);
}
