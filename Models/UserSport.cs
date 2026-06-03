using System;

namespace WorkoutRag.Models;

public class UserSport
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SportName { get; set; } = default!; // "Basketball", "Tennis", "Swimming", etc.
    public int PriorityLevel { get; set; } // 1 = Main Sport, 2 = Secondary Hobby
    public string SeasonStatus { get; set; } = default!; // "Off-season", "In-season"

    // Navigation properties
    public User User { get; set; } = default!;
}
