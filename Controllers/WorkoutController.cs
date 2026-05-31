using Microsoft.AspNetCore.Mvc;
using WorkoutRag.DTO;
using WorkoutRag.Services;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutController : ControllerBase
{
    private readonly WorkoutRetrievalService _retrievalService;
    private readonly OllamaService _ollamaService;

    public WorkoutController(WorkoutRetrievalService retrievalService, OllamaService ollamaService)
    {
        _retrievalService = retrievalService;
        _ollamaService = ollamaService;
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
            // 1. RETRIEVAL: Vector search for matching exercises
            var exercises = await _retrievalService.SearchExercisesAsync(
                request.Prompt,
                request.Equipment
            );

            if (!exercises.Any())
                return NotFound("No exercises found matching your equipment.");

            // 2. GENERATION: Send results to LLM for workout plan
            var workoutJson = await _ollamaService.GenerateWorkoutPlanAsync(
                request.Prompt,
                exercises
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
