using WorkoutRag.DTO;
using WorkoutRag.Models;
namespace WorkoutRag.Interfaces;
public interface IUserService
{
    Task<User> RegisterUserAsync(RegisterRequest request);
    Task<User?> CheckLoginAsync(LoginRequest request);
    Task<FitnessAssessmentResult> UpdateAthleticBaselineAsync(Guid userId, BenchmarkTestRequest request);
    Task<List<string>> UpdateLifestyleProfileAsync(Guid userId, UserLifestyleRequest request);
    Task<UserProfileResponse> GetUserProfileAsync(Guid userId);
    Task<UserProfileResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);
    Task<List<AdminUserResponse>> GetUsersAsync();
    Task<AdminUserResponse> UpdateUserRoleAsync(Guid id, UpdateUserRoleRequest request);
    Task DeleteUserAsync(Guid id);
}
