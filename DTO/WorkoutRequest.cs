namespace WorkoutRag.DTO;

public class WorkoutRequest
{
    // Session toggles
    public bool UseRag { get; set; } = true;

    // All spec-required fields
    public int Age { get; set; }
    public float Height { get; set; }
    public float Weight { get; set; }
    public float BMI { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
    public string MentalHealth { get; set; } = string.Empty;
    public string ExerciseFrequency { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string FitnessLevel { get; set; } = string.Empty;
    public string AvailableEquipment { get; set; } = string.Empty;
    public int WorkoutDuration { get; set; } = 45;
    public string PreviousInjury { get; set; } = string.Empty;
    public string AdditionalRequirements { get; set; } = string.Empty;
}
