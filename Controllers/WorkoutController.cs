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
public class WorkoutController : ControllerBase
{
    private readonly WorkoutRetrievalService _retrievalService;
    private readonly OllamaService _ollamaService;
    private readonly IUserRepository _userRepository;

    public WorkoutController(
        WorkoutRetrievalService retrievalService,
        OllamaService ollamaService,
        IUserRepository userRepository
    )
    {
        _retrievalService = retrievalService;
        _ollamaService = ollamaService;
        _userRepository = userRepository;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateWorkout([FromBody] WorkoutRequest request)
    {
        if (
            string.IsNullOrWhiteSpace(request.Prompt)
            || string.IsNullOrWhiteSpace(request.Equipment)
        )
        {
            return BadRequest("Prompt and Equipment are required.");
        }

        try
        {
            // 1. Extract the UserId directly from their secure JWT Token
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);
            //1. Fetch the user to get their Biomechanical needs
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");
            // 1. RETRIEVAL: Vector search for matching exercises
            var exercises = await _retrievalService.SearchExercisesAsync(
                request.Prompt,
                request.Equipment
            );

            if (!exercises.Any())
                return NotFound("No exercises found matching your equipment.");

            // 2. GENERATION: Send results to LLM for workout plan
            var workoutJson = await _ollamaService.GenerateWorkoutPlanAsync(
                request.Prompt, // 1. The userGoal
                request.Equipment, // 2. The new equipment string we just added
                exercises, // 3. The vector search results
                user // 4. The user object (which the error says is missing!)
            );

            // 3. Return formatted JSON
            return Content(workoutJson, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
