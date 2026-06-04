namespace WorkoutRag.DTO;
public class WorkoutRequest
{
    public Guid UserId {get;set;} //Know who is asking
    public string Prompt { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
}
