using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutRag.DTO;
using WorkoutRag.Interfaces;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutController : ControllerBase
{
    private readonly IWorkoutService _workoutService;

    public WorkoutController(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateWorkout([FromBody] WorkoutRequest request)
    {
        if (
            string.IsNullOrWhiteSpace(request.Goal)
            || string.IsNullOrWhiteSpace(request.AvailableEquipment)
        )
        {
            return BadRequest("Goal and AvailableEquipment are required.");
        }

        try
        {
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);

            var workoutJson = await _workoutService.GenerateWorkoutAsync(userId, request);

            return Content(workoutJson, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveWorkout([FromBody] SaveWorkoutRequest request)
    {
        try
        {
            var userId = Guid.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );
            await _workoutService.SaveWorkoutAsync(
                userId,
                request.Prompt,
                request.Equipment,
                request.WorkoutJson
            );

            return Ok(new { message = "Workout saved successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

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