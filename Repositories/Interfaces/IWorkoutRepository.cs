using WorkoutRag.Models;

namespace WorkoutRag.Repositories.Interfaces;

public interface IWorkoutRepository : IRepository<WorkoutHistory>
{
    Task<List<WorkoutHistory>> GetByUserIdAsync(Guid userId);
}