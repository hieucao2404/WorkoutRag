using Microsoft.AspNetCore.Mvc;
using WorkoutRag.DTO;
using WorkoutRag.Models;
using WorkoutRag.Repositories;
using WorkoutRag.Services;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OnboardingController : ControllerBase
{
    private readonly IRepository<User> _userRepository;

    public OnboardingController(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("benchmark")]
    public async Task<IActionResult> CreateUserFromBenchmarks(
        [FromBody] BenchmarkTestRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest("Username is required.");
        }

        try
        {
            //1. Run the data through Engine
            var assessment = AthleticLevelCalculator.CalculateAssessment(request);

            //2. Map the data to a new User entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Age = request.Age,
                WeightKg = request.WeightKg,
                HeightCm = request.HeightCm,

                // Save the calculated level
                AthleticLevel = assessment.Level,
                // Save the raw physical benchmarks
                // PushUpsMax = request.PushUpsMax,
                // PullUpsMax = request.PullUpsMax,
                // PlankHoldSeconds = request.PlankHoldSeconds,

                //Initialize the AI Promps array directly with the weak/strong areas
                ComputedBiomechanicalNeeds = new List<string>(),
            };

            //Conver weak areas into immediate AI directives
            if (assessment.WeakAreas.Any())
            {
                user.ComputedBiomechanicalNeeds.Add(
                    $"[Programming] Athlete has identified weaknesses in: {string.Join(", ", assessment.WeakAreas)}. Prioritize these areas."
                );
            }

            // 3. Save to PostgresSQL via entity framework
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            //4.  Return the generated ID and the rich assessment object
            return Ok(
                new
                {
                    message = "User profile and athletic baseline established successfully.",
                    userId = user.Id,
                    assessment = assessment,
                }
            );
        }
        catch (Exception ex)
        {
            //Log the exception in a read production environment
            return StatusCode(500, $"An error occured during onboarding: {ex.Message}");
        }
    }

    [HttpPost("lifestyle")]
    public async Task<IActionResult> UpdateLifestyleProfile([FromBody] UserLifestyleRequest request)
    {
        try
        {
            //We need to fetch the user and include their existing LifestyleProfie
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                return NotFound("User not found.");
            // Map the DTO to the Owned Entitues
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

            //Runt the Biomechanical analyzer
            var lifestyleNeeds = BiomechanicalAnalyzer.CalculateNeeds(user.LifestyleProfile);

            //Append these new needs to the existing ones
            user.ComputedBiomechanicalNeeds.AddRange(lifestyleNeeds);

            //Remove duplicates just incase
            user.ComputedBiomechanicalNeeds = user.ComputedBiomechanicalNeeds.Distinct().ToList();

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return Ok(
                new
                {
                    message = "Lifestyle profile saved.",
                    computedNeeds = user.ComputedBiomechanicalNeeds,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occured: {ex.Message}");
        }
    }
}
