using System.Security.Claims;
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
    private readonly WorkoutService _workoutService;

    public WorkoutController(
        WorkoutRetrievalService retrievalService,
        OllamaService ollamaService,
        IUserRepository userRepository,
        WorkoutService workoutService
    )
    {
        _retrievalService = retrievalService;
        _ollamaService = ollamaService;
        _userRepository = userRepository;
        _workoutService = workoutService;
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
            // Extract UserId from JWT token (not from request body)
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);

            // Pass to service
            var workoutJson = await _workoutService.GenerateAndSaveWorkoutAsync(userId, request);

            return Content(workoutJson, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // NEW ENDPOINT: Fetch logs directly for the Frontend Dashboard
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var history = await _workoutService.GetUserWorkoutHistoryAsync(userId);

            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
