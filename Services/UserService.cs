namespace WorkoutRag.Services;

using System;
using System.Collections.Generic;
using WorkoutRag.DTO;
using WorkoutRag.Interfaces;
using WorkoutRag.Models;
using WorkoutRag.Repositories.Interfaces;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> RegisterUserAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByUsernameOrEmailAsync(
            request.Username,
            request.Email
        );

        if (existingUser != null)
        {
            throw new Exception("Username or email already exists");
        }

        var User = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer,
            ComputedBiomechanicalNeeds = new List<string>(),
        };

        await _userRepository.AddAsync(User);
        await _userRepository.SaveChangesAsync();

        return User;
    }

    public async Task<User?> CheckLoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(
            request.Username,
            request.Username
        );

        if (
            user == null
            || string.IsNullOrEmpty(user.PasswordHash)
            || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
        )
        {
            return null; // Invalid credentials
        }

        return user;
    }

    public async Task<FitnessAssessmentResult> UpdateAthleticBaselineAsync(
        Guid userId,
        BenchmarkTestRequest request
    )
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        // Run the math engine
        var assessment = AthleticLevelCalculator.CalculateAssessment(request);

        // Update the user
        user.Age = request.Age;
        user.WeightKg = request.WeightKg;
        user.HeightCm = request.HeightCm;
        user.AthleticLevel = assessment.Level;
        user.ComputedBiomechanicalNeeds ??= new List<string>();

        if (assessment.WeakAreas.Any())
        {
            user.ComputedBiomechanicalNeeds.Add(
                $"[Programming] Athlete has identified weaknesses in: {string.Join(", ", assessment.WeakAreas)}. Prioritize these areas."
            );
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return assessment;
    }

    public async Task<List<string>> UpdateLifestyleProfileAsync(
        Guid userId,
        UserLifestyleRequest request
    )
    {
        // Use GetByIdWithProfileAsync so LifestyleProfile is loaded into the tracker
        var user = await _userRepository.GetByIdWithProfileAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        if (user.LifestyleProfile == null)
        {
            // First time — INSERT a new profile
            user.LifestyleProfile = new UserLifestyleProfile
            {
                UserId = user.Id,
                Occupation = request.Occupation,
                Movement = request.Movement,
                Stressors = request.Stressors,
                Recovery = request.Recovery,
                Habits = request.Habits,
                Pain = request.Pain,
            };
        }
        else
        {
            // Profile already exists — UPDATE in-place to avoid unique constraint violation
            user.LifestyleProfile.Occupation = request.Occupation;
            user.LifestyleProfile.Movement = request.Movement;
            user.LifestyleProfile.Stressors = request.Stressors;
            user.LifestyleProfile.Recovery = request.Recovery;
            user.LifestyleProfile.Habits = request.Habits;
            user.LifestyleProfile.Pain = request.Pain;
        }

        // Reset and recalculate biomechanical needs from scratch
        user.ComputedBiomechanicalNeeds = BiomechanicalAnalyzer
            .CalculateNeeds(user.LifestyleProfile)
            .Distinct()
            .ToList();

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return user.ComputedBiomechanicalNeeds;
    }

    public async Task<UserProfileResponse> GetUserProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        UserLifestyleProfileResponse? lifestyleProfileResponse = null;

        if (user.LifestyleProfile != null)
        {
            lifestyleProfileResponse = new UserLifestyleProfileResponse
            {
                Id = user.LifestyleProfile.Id,
                UserId = user.LifestyleProfile.UserId,
                Occupation = user.LifestyleProfile.Occupation,
                Movement = user.LifestyleProfile.Movement,
                Stressors = user.LifestyleProfile.Stressors,
                Recovery = user.LifestyleProfile.Recovery,
                Habits = user.LifestyleProfile.Habits,
                Pain = user.LifestyleProfile.Pain,
            };
        }

        return new UserProfileResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Age = user.Age,
            WeightKg = user.WeightKg,
            HeightCm = user.HeightCm,
            Gender = user.Gender,
            AthleticLevel = user.AthleticLevel,
            LifestyleProfile = lifestyleProfileResponse,
            ComputedBiomechanicalNeeds = user.ComputedBiomechanicalNeeds,
            CreatedAt = user.CreatedAt,
        };
    }

    public async Task<UserProfileResponse> UpdateUserProfileAsync(
        Guid userId,
        UpdateUserProfileRequest request
    )
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        if (request.Age.HasValue)
            user.Age = request.Age;

        if (request.WeightKg.HasValue)
            user.WeightKg = request.WeightKg;

        if (request.HeightCm.HasValue)
            user.HeightCm = request.HeightCm;

        if (!string.IsNullOrEmpty(request.Gender))
            user.Gender = request.Gender;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return await GetUserProfileAsync(userId);
    }

    public async Task<List<AdminUserResponse>> GetUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.OrderBy(u => u.Username).Select(ToAdminUserResponse).ToList();
    }

    public async Task<AdminUserResponse> UpdateUserRoleAsync(
        Guid id,
        UpdateUserRoleRequest request
    )
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.Role = request.Role;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return ToAdminUserResponse(user);
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

    private static AdminUserResponse ToAdminUserResponse(User user)
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
}
