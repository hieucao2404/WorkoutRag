using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> CreateUserFromBenchmarks(
        [FromBody] BenchmarkTestRequest request
    )
    {
        try
        {
            //1. Extract the UserId directly from their secure JWT Token
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);

            // 2. Fetch the user that the AuthController just created
            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
                return NotFound("User account not found.");

            // 3. Run the calculation
            var assessment = AthleticLevelCalculator.CalculateAssessment(request);
            existingUser.Age = request.Age;
            existingUser.WeightKg = request.WeightKg;
            existingUser.HeightCm = request.HeightCm;
            existingUser.AthleticLevel = assessment.Level;
            existingUser.ComputedBiomechanicalNeeds ??= new List<string>();

            //Conver weak areas into immediate AI directives
            if (assessment.WeakAreas.Any())
            {
                existingUser.ComputedBiomechanicalNeeds.Add(
                    $"[Programming] Athlete has identified weaknesses in: {string.Join(", ", assessment.WeakAreas)}. Prioritize these areas."
                );
            }

            // 3. Save to PostgresSQL via entity framework
            await _userRepository.UpdateAsync(existingUser);
            await _userRepository.SaveChangesAsync();

            //4.  Return the generated ID and the rich assessment object
            return Ok(
                new
                {
                    message = "User profile and athletic baseline established successfully.",
                    userId = existingUser.Id,
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
