namespace WorkoutRag.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public int? Age { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeighCm { get; set; }
    public string? DailyPosture { get; set; }
    public List<string> KnownImbalances { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Navigation properties
    public ICollection<WorkoutHistory> WorkoutHistories { get; set; } = new List<WorkoutHistory>();
    public ICollection<UserSport> Sports { get; set; } = new List<UserSport>();
    public ICollection<UserDiet> Diets { get; set; } = new List<UserDiet>();
}
