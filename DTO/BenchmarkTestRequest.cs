namespace WorkoutRag.DTO;

public class BenchmarkTestRequest
{
    public string Username { get; set; } = string.Empty;
    public string Gender { get; set; } = "Male";

    //Raw test data
    public int PushUpsMax { get; set; } // Unbroken strict push-ups
    public int PullUpsMax { get; set; }
    public int PlankHoldSeconds { get; set; } //Forarm plank in seconds
    public int SquatReps { get; set; }

    // Power Metric (Explosiveness)
    public int BroadJumpCm { get; set; } // Distance jumped forward from a standstill

    public int OneKmRunTimeSeconds { get; set; } // E.g., 8 mins = 480 seconds


    // Mobility Metrics (Simple Pass/Fail tests)
    public bool CanDeepSquat { get; set; } // Hips below knees without heels lifting
    public bool CanTouchToes { get; set; } // Legs straight, touching floor
    public bool CanPerformWallSlide { get; set; }
    public bool CanReachHandsBehindBack { get; set; }

    // Recovery Metrics (Can be pulled from their Lifestyle profile later, but needed for the score)
    public int AverageSleepHours { get; set; }
    public int StressLevel { get; set; } // 1-10

    //Basic info
    public int? Age { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
}
