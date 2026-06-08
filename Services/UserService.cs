namespace WorkoutRag.Services;

using System;
using System.Collections.Generic;
using WorkoutRag.DTO;
using WorkoutRag.Models;
using WorkoutRag.Repositories;

public class UserService
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
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

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

        var lifestyleNeeds = BiomechanicalAnalyzer.CalculateNeeds(user.LifestyleProfile);
        user.ComputedBiomechanicalNeeds.AddRange(lifestyleNeeds);
        user.ComputedBiomechanicalNeeds = user.ComputedBiomechanicalNeeds.Distinct().ToList();

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return user.ComputedBiomechanicalNeeds;
    }
}
