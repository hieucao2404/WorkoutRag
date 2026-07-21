using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutRag.DTO;
using WorkoutRag.Interfaces;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<AdminUserResponse>>> GetUsersForAdmin()
    {
        return Ok(await _userService.GetUsersAsync());
    }

    [HttpDelete("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUserForAdmin(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return Ok(new { message = "User deleted", userId = id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get the authenticated user's complete profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);
            var profile = await _userService.GetUserProfileAsync(userId);

            return Ok(profile);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update user's physical profile (Age, Weight, Height)
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);
            var updatedProfile = await _userService.UpdateUserProfileAsync(userId, request);

            return Ok(updatedProfile);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update user's lifestyle profile
    /// </summary>
    [HttpPut("lifestyle")]
    public async Task<IActionResult> UpdateLifestyleProfile([FromBody] UserLifestyleRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            var userId = Guid.Parse(userIdClaim);
            var biomechanicalNeeds = await _userService.UpdateLifestyleProfileAsync(
                userId,
                request
            );

            return Ok(new { message = "Lifestyle profile updated", biomechanicalNeeds });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // /// <summary>
    // /// Get user's lifestyle profile
    // /// </summary>
    // [HttpGet("lifestyle")]
    // public async Task<IActionResult> GetLifestyleProfile()
    // {
    //     try
    //     {
    //         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //         if (userIdClaim == null)
    //             return Unauthorized("Invalid token.");

    //         var userId = Guid.Parse(userIdClaim);
    //         var lifestyleProfile = await _userService.GetUserLifestyleProfileAsync(userId);

    //         return Ok(lifestyleProfile);
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    // }
}
