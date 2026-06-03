namespace WorkoutRag.DTO;

public class FitnessAssessmentResult
{
    public double TotalScore {get;set;} // 0 - 100
    public string Level {get;set;} = string.Empty;

    //Sub-scores 0 - 100
    public double UpperPushScore {get;set;}
    public double UpperPullScore {get;set;}
    public double CoreScore{get;set;}
    public double LowerBodyScore{get;set;}

    public List<string> WeakAreas{get;set;} = new();
    public List<string> StrongAreas{get;set;} = new();
}
