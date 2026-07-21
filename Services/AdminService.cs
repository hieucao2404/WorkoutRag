using WorkoutRag.DTO;
using WorkoutRag.Interfaces;
using WorkoutRag.Models;
using WorkoutRag.Repositories.Interfaces;

namespace WorkoutRag.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IWorkoutRepository _workoutRepository;

    public AdminService(
        IUserRepository userRepository,
        IExerciseRepository exerciseRepository,
        IWorkoutRepository workoutRepository
    )
    {
        _userRepository = userRepository;
        _exerciseRepository = exerciseRepository;
        _workoutRepository = workoutRepository;
    }

    public async Task<List<AdminUserResponse>> GetUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users
            .OrderBy(u => u.Username)
            .Select(ToUserResponse)
            .ToList();
    }

    public async Task<AdminUserResponse> UpdateUserRoleAsync(Guid id, UpdateUserRoleRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.Role = request.Role;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return ToUserResponse(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<List<AdminExerciseResponse>> GetExercisesAsync()
    {
        var exercises = await _exerciseRepository.GetAllAsync();

        return exercises
            .OrderBy(e => e.Name)
            .Select(ToExerciseResponse)
            .ToList();
    }

    public async Task<AdminExerciseResponse> CreateExerciseAsync(AdminExerciseRequest request)
    {
        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Equipment = request.Equipment,
            DifficultyLevel = request.DifficultyLevel,
            MovementPattern = request.MovementPattern,
            ExerciseType = request.ExerciseType,
            MusclesTargeted = request.MusclesTargeted,
            CreatedAt = DateTime.UtcNow,
        };

        await _exerciseRepository.AddAsync(exercise);
        await _exerciseRepository.SaveChangesAsync();

        return ToExerciseResponse(exercise);
    }

    public async Task<AdminExerciseResponse> UpdateExerciseAsync(
        Guid id,
        AdminExerciseRequest request
    )
    {
        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise == null)
        {
            throw new KeyNotFoundException("Exercise not found.");
        }

        exercise.Name = request.Name;
        exercise.Description = request.Description;
        exercise.Equipment = request.Equipment;
        exercise.DifficultyLevel = request.DifficultyLevel;
        exercise.MovementPattern = request.MovementPattern;
        exercise.ExerciseType = request.ExerciseType;
        exercise.MusclesTargeted = request.MusclesTargeted;

        await _exerciseRepository.UpdateAsync(exercise);
        await _exerciseRepository.SaveChangesAsync();

        return ToExerciseResponse(exercise);
    }

    public async Task DeleteExerciseAsync(Guid id)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise == null)
        {
            throw new KeyNotFoundException("Exercise not found.");
        }

        await _exerciseRepository.DeleteAsync(exercise);
        await _exerciseRepository.SaveChangesAsync();
    }

    public async Task<List<AdminWorkoutResponse>> GetWorkoutsAsync()
    {
        var workouts = await _workoutRepository.GetAllWithUsersAsync();

        return workouts
            .Select(w => new AdminWorkoutResponse
            {
                Id = w.Id,
                UserId = w.UserId,
                Username = w.User.Username,
                Email = w.User.Email,
                UserPrompt = w.UserPrompt,
                EquipmentFilter = w.EquipmentFilter,
                RawAiJson = w.RawAiJson,
                CreatedAt = w.CreatedAt,
            })
            .ToList();
    }

    private static AdminUserResponse ToUserResponse(User user)
    {
        return new AdminUserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            AthleticLevel = user.AthleticLevel,
            CreatedAt = user.CreatedAt,
        };
    }

    private static AdminExerciseResponse ToExerciseResponse(Exercise exercise)
    {
        return new AdminExerciseResponse
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Equipment = exercise.Equipment,
            DifficultyLevel = exercise.DifficultyLevel,
            MovementPattern = exercise.MovementPattern,
            ExerciseType = exercise.ExerciseType,
            MusclesTargeted = exercise.MusclesTargeted,
            CreatedAt = exercise.CreatedAt,
        };
    }
}
