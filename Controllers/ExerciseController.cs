using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutRag.DTO;
using WorkoutRag.Interfaces;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ExerciseController : ControllerBase
{
    private readonly IExerciseService _exerciseService;

    public ExerciseController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminExerciseResponse>>> GetExercises()
    {
        return Ok(await _exerciseService.GetExercisesAsync());
    }

    [HttpPost]
    public async Task<ActionResult<AdminExerciseResponse>> CreateExercise(
        [FromBody] AdminExerciseRequest request
    )
    {
        var exercise = await _exerciseService.CreateExerciseAsync(request);
        return CreatedAtAction(nameof(GetExercises), new { id = exercise.Id }, exercise);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExercise(Guid id, [FromBody] AdminExerciseRequest request)
    {
        try
        {
            var exercise = await _exerciseService.UpdateExerciseAsync(id, request);
            return Ok(exercise);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExercise(Guid id)
    {
        try
        {
            await _exerciseService.DeleteExerciseAsync(id);
            return Ok(new { message = "Exercise deleted", exerciseId = id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
