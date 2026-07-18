using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutRag.DTO;
using WorkoutRag.Interfaces;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserResponse>>> GetUsers()
    {
        return Ok(await _adminService.GetUsersAsync());
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var user = await _adminService.UpdateUserRoleAsync(id, request);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _adminService.DeleteUserAsync(id);
            return Ok(new { message = "User deleted", userId = id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("exercises")]
    public async Task<ActionResult<List<AdminExerciseResponse>>> GetExercises()
    {
        return Ok(await _adminService.GetExercisesAsync());
    }

    [HttpPost("exercises")]
    public async Task<ActionResult<AdminExerciseResponse>> CreateExercise(
        [FromBody] AdminExerciseRequest request
    )
    {
        var exercise = await _adminService.CreateExerciseAsync(request);
        return CreatedAtAction(nameof(GetExercises), new { id = exercise.Id }, exercise);
    }

    [HttpPut("exercises/{id:guid}")]
    public async Task<IActionResult> UpdateExercise(Guid id, [FromBody] AdminExerciseRequest request)
    {
        try
        {
            var exercise = await _adminService.UpdateExerciseAsync(id, request);
            return Ok(exercise);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("exercises/{id:guid}")]
    public async Task<IActionResult> DeleteExercise(Guid id)
    {
        try
        {
            await _adminService.DeleteExerciseAsync(id);
            return Ok(new { message = "Exercise deleted", exerciseId = id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("workouts")]
    public async Task<ActionResult<List<AdminWorkoutResponse>>> GetWorkouts()
    {
        return Ok(await _adminService.GetWorkoutsAsync());
    }
}
