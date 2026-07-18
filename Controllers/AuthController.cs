using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorkoutRag.DTO;
using WorkoutRag.Models;
using WorkoutRag.Interfaces;

namespace WorkoutRag.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _config;

    public AuthController(IUserService userService, IConfiguration config)
    {
        _userService = userService;
        _config = config;
    }

    [HttpGet("admin/status")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminStatus()
    {
        return Ok(new { message = "Admin access granted" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // The service handles checking duplicates, hashing the password, and saving to the DB.
            var user = await _userService.RegisterUserAsync(request);

            return Ok(new { message = "User registered successfully", userId = user.Id, role = user.Role });
        }
        catch (Exception ex)
        {
            // If the service throws "Username is already taken", we catch it and return a 400.
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) // Fixed typo (was 'Logic')
    {
        // The service fetches the user and verifies the BCrypt hash for us.
        var user = await _userService.CheckLoginAsync(request);

        if (user == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Only the web-specific logic (JWT generation) stays in the controller.
        var token = GenerateJwtToken(user);

        return Ok(
            new
            {
                message = "Login successful",
                token = token,
                userId = user.Id,
                role = user.Role,
            }
        );
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //Claims are pieces of data embedded directly inside the token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
