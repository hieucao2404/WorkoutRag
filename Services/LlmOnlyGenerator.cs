using WorkoutRag.Interfaces;
using WorkoutRag.DTO;

namespace WorkoutRag.Services;

public class LlmOnlyGenerator : IWorkoutGenerator
{
    private readonly IOllamaService _ollamaService;

    public LlmOnlyGenerator(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    public async Task<string> GenerateAsync(WorkoutGenerationContext context)
    {
        // Notice: NO retrieved exercises are included in this prompt.
        var prompt = $@"You are an elite clinical strength coach. Design a safe workout plan.
            
            USER PROFILE:
            Age: {context.Age}, BMI: {context.BMI}, Fitness Level: {context.FitnessLevel}
            Goal: {context.Goal}, Injuries: {context.PreviousInjury}
            Equipment: {context.AvailableEquipment}, Duration: {context.WorkoutDuration} mins
            Your response MUST perfectly match this JSON schema:
            {{
                ""workoutName"": ""String"",
                ""duration"": 45,
                ""goal"": ""String"",
                ""exercises"": [
                    {{
                        ""name"": ""String"",
                        ""sets"": 4,
                        ""reps"": ""String (e.g., 8-12)"",
                        ""rest"": ""String (e.g., 90 sec)"",
                        ""notes"": ""String""
                    }}
                ]
            }}";
        return await _ollamaService.SendPromptAsync(prompt);
}
}