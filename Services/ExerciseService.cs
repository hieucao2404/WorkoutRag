using WorkoutRag.DTO;
using WorkoutRag.Interfaces;
using WorkoutRag.Models;
using WorkoutRag.Repositories.Interfaces;

namespace WorkoutRag.Services;

public class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;

    public ExerciseService(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<List<AdminExerciseResponse>> GetExercisesAsync()
    {
        var exercises = await _exerciseRepository.GetAllAsync();

        return exercises.OrderBy(e => e.Name).Select(ToExerciseResponse).ToList();
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
