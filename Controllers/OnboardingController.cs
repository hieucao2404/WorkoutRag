using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutRag.DTO;
using WorkoutRag.Interfaces;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IUserService _userService;

    public OnboardingController(IUserService userService)
    {
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
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);

            // Delegate logic entirely to the service layer
            var assessment = await _userService.UpdateAthleticBaselineAsync(userId, request);

            return Ok(
                new
                {
                    message = "User profile and athletic baseline established successfully.",
                    userId = userId,
                    assessment = assessment,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred during onboarding: {ex.Message}");
        }
    }

    [HttpPost("lifestyle")]
    public async Task<IActionResult> UpdateLifestyleProfile([FromBody] UserLifestyleRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);

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