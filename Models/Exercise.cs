using System;
using System.Collections.Generic;
using Pgvector;

namespace WorkoutRag.Models;

public class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Equipment { get; set; } = default!;
    public string DifficultyLevel { get; set; } = default!; // "Beginner", "Intermediate", "Advanced"
    public string MovementPattern { get; set; } = default!; // "Push", "Pull", "Squat", "Hinge", "Core"
    public string ExerciseType { get; set; } = default!; // "Compound", "Isolation", "Plyometric"
    public List<string> MusclesTargeted { get; set; } = new();

    public Vector? Embedding { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } =
        new List<WorkoutExercise>();
}
