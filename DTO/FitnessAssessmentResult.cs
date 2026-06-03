namespace WorkoutRag.DTO;

public class FitnessAssessmentResult
{
    public double TotalScore { get; set; }
    public string Level { get; set; } = string.Empty;

    // The 5 Pillars (0-100 scales)
    public double StrengthScore { get; set; }
    public double PowerScore { get; set; }
    public double EnduranceScore { get; set; }
    public double MobilityScore { get; set; }
    public double RecoveryScore { get; set; }

    public List<string> WeakAreas { get; set; } = new();
    public List<string> StrongAreas { get; set; } = new();
}
