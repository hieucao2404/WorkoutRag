using System;
using System.Collections.Generic; // FIXED: Typo (Colections -> Collections)
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkoutRag.Data;
using WorkoutRag.Models;

namespace WorkoutRag.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    // 1. Implementation for finding a user by their unique username
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByIdWithProfileAsync(Guid id)
    {
        return await _context
            .Users.Include(u => u.LifestyleProfile) // This pulls in the flat Owned Entities!
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string username, string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u =>
            u.Username == username || u.Email == email
        );
    }

    // public async Task<List<WorkoutHistory>> GetWorkoutHistoryAsync(Guid userId)
    // {
    //     return await _context
    //         .WorkoutHistories.Where(h => h.UserId == userId)
    //         .OrderByDescending(h => h.) // Puts the newest workouts at the top of the dashboard
    //         .ToListAsync();
    // }
}
