using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pgvector;
using WorkoutRag.Models;

namespace WorkoutRag.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);

    // We need this specific method so EF Core knows to load the [Owned] Lifestyle profile
    Task<User?> GetByIdWithProfileAsync(Guid id);
    Task<User?> GetByUsernameOrEmailAsync(string username, string email);
}
