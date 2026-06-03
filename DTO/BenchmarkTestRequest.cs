namespace WorkoutRag.DTO;

public class BenchmarkTestRequest{
    public string Username {get; set;} = string.Empty;

    //Raw test data
    public int PushUpsMax{get;set;} // Unbroken strict push-ups
    public int PullUpsMax{get;set;}
    public int PlankHoldSeconds{get;set;} //Forarm plank in seconds
    public int SquatReps{get;set;}

    //Basic info
    public int? Age {get;set;}
    public decimal? WeightKg {get;set;}
    public decimal? HeightCm {get;set;}
}
