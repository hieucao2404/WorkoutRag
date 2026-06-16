using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutRag.DTO;
using WorkoutRag.Models;
using WorkoutRag.Repositories;
using WorkoutRag.Services;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly UserService _userService;

    public OnboardingController(IRepository<User> userRepository, UserService userService)
    {
        _userRepository = userRepository;
        _userService = userService;
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
            // Extract UserId from JWT token instead of request
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);

            // Let the UserService handle the EF Core mapping and the Biomechanical Math!
            var computedNeeds = await _userService.UpdateLifestyleProfileAsync(userId, request);

            return Ok(
                new
                {
                    message = "Lifestyle profile updated",
                    biomechanicalDirectives = computedNeeds,
                }
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
