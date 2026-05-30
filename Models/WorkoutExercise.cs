using System;

namespace WorkoutRag.Models;

public class WorkoutExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutId { get; set; }
    public Guid ExerciseId { get; set; }
    public int RecommendedSets { get; set; }
    public string RecommendedReps { get; set; } = default!;

    // Navigation properties
    public WorkoutHistory Workout { get; set; } = default!;
    public Exercise Exercise { get; set; } = default!;
}
