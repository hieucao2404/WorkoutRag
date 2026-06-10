using WorkoutRag.Models;

namespace WorkoutRag.Repositories;

public interface IWorkoutRepository : IRepository<WorkoutHistory>
{
    Task<List<WorkoutHistory>> GetByUserIdAsync(Guid userId);
}