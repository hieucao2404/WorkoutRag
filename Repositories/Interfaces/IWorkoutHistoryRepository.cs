using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pgvector;
using WorkoutRag.Models;

namespace WorkoutRag.Repositories.Interfaces;

public interface IWorkoutHistoryRepository : IRepository<User>
{
    Task<List<WorkoutHistory>> GetWorkoutHistoryAsync(Guid userId);
}
