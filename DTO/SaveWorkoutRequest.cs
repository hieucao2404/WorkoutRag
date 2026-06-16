namespace WorkoutRag.DTO;

public class SaveWorkoutRequest
{
    public string Prompt {get;set;} = string.Empty;
    public string Equipment{get;set;} = string.Empty;
    public string WorkoutJson{get;set;} = string.Empty;
}
