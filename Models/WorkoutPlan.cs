using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.VectorData;

namespace WorkoutRag.Models;

public class WorkoutPlan
{
    [JsonPropertyName("plan_title")]
    public string PlanTitle { get; set; } = default!;

    [JsonPropertyName("workouts")]
    public List<WorkoutDay> Workouts { get; set; } = default!;
}

public class WorkoutDay
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("exercises")]
    public List<ExerciseAllocation> Exercises { get; set; } = default!;
}

public class ExerciseAllocation
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("sets")]
    public int Sets { get; set; }

    [JsonPropertyName("rep_range")]
    public string RepRange { get; set; } = default!;
}
