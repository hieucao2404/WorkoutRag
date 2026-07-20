namespace WorkoutRag.DTO;

public class WorkoutGenerationContext
{
    //Demographics and Biometrics
    public int Age {get;set;}
    public float Height{get;set;}
    public float Weight {get;set;}
    public float BMI {get;set;}

    //Lifestyle and Fitness Profile
    public string ActivityLevel{get;set;} = string.Empty;
    public string FitnessLevel {get;set;} = string.Empty;
    public string Goal {get;set;} = string.Empty;
    public string ExerciseFrequency {get;set;} = string.Empty;
    public string PreviousInjury{get;set;} = string.Empty;
    public string MentalHealth{get;set;} = string.Empty;

    //Session Context
    public string AvailableEquipment {get;set;} = string.Empty;
    public int WorkoutDuration {get;set;}
    public string AdditionalRequirements {get;set;} = string.Empty;

    public string? PreviousWorkoutJson{get;set;}
    public string? UserFeedback{get;set;}

    public bool IsAdjustmentRequest => !string.IsNullOrWhiteSpace(PreviousWorkoutJson) && !string.IsNullOrWhiteSpace(UserFeedback);
}