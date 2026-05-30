using System;

namespace WorkoutRag.Models;

public class WorkoutHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserPrompt { get; set; } = default!;
    public string EquipmentFilter { get; set; } = default!;
    public string? RawAiJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Navigatioon Properite
    public User User { get; set; } = default!;
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
