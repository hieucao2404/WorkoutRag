using WorkoutRag.Models;

namespace WorkoutRag.DTO;

public class UserLifestyleRequest
{
    public Guid UserId { get; set; }
    public OccupationType Occupation { get; set; }
    public DailyMovementProfile Movement { get; set; } = new();
    public OccupationalStressProfile Stressors { get; set; } = new();
    public RecoveryProfile Recovery { get; set; } = new();
    public DailyHabitProfile Habits { get; set; } = new();
    public PainAssessment Pain { get; set; } = new();
}
