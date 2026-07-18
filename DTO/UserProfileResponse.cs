using WorkoutRag.Models;

namespace WorkoutRag.DTO;

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;

    // Physical Profile
    public int? Age { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string AthleticLevel { get; set; } = string.Empty;
    public string? Gender {get;set;}

    // Lifestyle Profile (if exists)
    public UserLifestyleProfileResponse? LifestyleProfile { get; set; }

    // AI-Generated needs
    public List<string> ComputedBiomechanicalNeeds { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}

public class UpdateUserProfileRequest
{
    public int? Age { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string? Gender{get;set;}
}
