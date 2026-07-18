using Microsoft.EntityFrameworkCore;
using WorkoutRag.Data;
using WorkoutRag.DTO;
using WorkoutRag.Interfaces;
using WorkoutRag.Models;

namespace WorkoutRag.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminUserResponse>> GetUsersAsync()
    {
        return await _context
            .Users.AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => new AdminUserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                AthleticLevel = u.AthleticLevel,
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task<AdminUserResponse> UpdateUserRoleAsync(Guid id, UpdateUserRoleRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.Role = request.Role;
        await _context.SaveChangesAsync();

        return ToUserResponse(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AdminExerciseResponse>> GetExercisesAsync()
    {
        return await _context
            .Exercises.AsNoTracking()
            .OrderBy(e => e.Name)
            .Select(e => ToExerciseResponse(e))
            .ToListAsync();
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

        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        return ToExerciseResponse(exercise);
    }

    public async Task<AdminExerciseResponse> UpdateExerciseAsync(
        Guid id,
        AdminExerciseRequest request
    )
    {
        var exercise = await _context.Exercises.FindAsync(id);
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

        await _context.SaveChangesAsync();

        return ToExerciseResponse(exercise);
    }

    public async Task DeleteExerciseAsync(Guid id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null)
        {
            throw new KeyNotFoundException("Exercise not found.");
        }

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AdminWorkoutResponse>> GetWorkoutsAsync()
    {
        return await _context
            .WorkoutHistories.AsNoTracking()
            .Include(w => w.User)
            .OrderByDescending(w => w.CreatedAt)
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
            .ToListAsync();
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
