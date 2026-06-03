namespace WorkoutRag.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public int? Age { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string? DailyPosture { get; set; }
    public List<string> KnownImbalances { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 1-to-1 Relationship with the new Lifestyle Profile
    public UserLifestyleProfile? LifestyleProfile { get; set; }

    // The AI's dynamically generated needs based on the profile
    public List<string> ComputedBiomechanicalNeeds { get; set; } = new();

    //Navigation properties
    public ICollection<WorkoutHistory> WorkoutHistories { get; set; } = new List<WorkoutHistory>();
    public ICollection<UserSport> Sports { get; set; } = new List<UserSport>();
    public ICollection<UserDiet> Diets { get; set; } = new List<UserDiet>();
}
