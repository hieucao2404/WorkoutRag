using Microsoft.EntityFrameworkCore;
using WorkoutRag.Data;
using WorkoutRag.Models;
using WorkoutRag.Repositories.Interfaces;

namespace WorkoutRag.Repositories.Implementations;

public class WorkoutRepository : Repository<WorkoutHistory>, IWorkoutRepository
{
    private readonly AppDbContext _context;

    public WorkoutRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<List<WorkoutHistory>> GetByUserIdAsync(Guid userId)
    {
        return await _context
            .WorkoutHistories.Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}
