using System;
using System.Collections.Generic;

namespace WorkoutRag.Models;

public class ExerciseRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Equipment { get; set; } = default!;
    public List<string> TargetMuscles { get; set; } = new();
    public ReadOnlyMemory<float> Embedding { get; set; }
}
