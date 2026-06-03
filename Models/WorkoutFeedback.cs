using System;
using System.Collections.Generic;

namespace WorkoutRag.Models;

public class WorkoutFeedback
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WorkoutHistoryId { get; set; }

    public int RPEScore { get; set; }

    // Using PostgreSQL native text[] arrays!
    public List<string> ExercisesCompleted { get; set; } = new();
    public List<string> ExercisesSkipped { get; set; } = new();
    public List<string> SorenessAreas { get; set; } = new();

    public int SorenessIntensity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
    public WorkoutHistory Workout { get; set; } = default!;
}
