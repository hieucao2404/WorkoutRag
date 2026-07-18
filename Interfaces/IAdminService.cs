using WorkoutRag.DTO;

namespace WorkoutRag.Interfaces;

public interface IAdminService
{
    Task<List<AdminUserResponse>> GetUsersAsync();
    Task<AdminUserResponse> UpdateUserRoleAsync(Guid id, UpdateUserRoleRequest request);
    Task DeleteUserAsync(Guid id);
    Task<List<AdminExerciseResponse>> GetExercisesAsync();
    Task<AdminExerciseResponse> CreateExerciseAsync(AdminExerciseRequest request);
    Task<AdminExerciseResponse> UpdateExerciseAsync(Guid id, AdminExerciseRequest request);
    Task DeleteExerciseAsync(Guid id);
    Task<List<AdminWorkoutResponse>> GetWorkoutsAsync();
}
