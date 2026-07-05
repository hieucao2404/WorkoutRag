using WorkoutRag.Interfaces;
using WorkoutRag.DTO;

namespace WorkoutRag.Services;

public class RagGenerator : IWorkoutGenerator
{
    private readonly OllamaService _ollamaService;
    private readonly WorkoutRetrievalService _retrievalService;

    public RagGenerator(OllamaService ollamaService, WorkoutRetrievalService retrievalService)
    {
        _ollamaService = ollamaService;
        _retrievalService = retrievalService;
    }

    public async Task<string> GenerateAsync(WorkoutGenerationContext context)
    {
        // 1. RAG STEP: Retrieve exercises based on the user's goal and equipment
        var retrievedExercises = await _retrievalService.SearchExercisesAsync(context.Goal, context.AvailableEquipment);
        var exerciseListString = string.Join("\n", retrievedExercises.Select(e => $"- {e.Name}: {e.Description}"));

        // 2. Prompt Generation
        var prompt = $@"You are an elite clinical strength coach. Design a safe workout plan.
            
            USER PROFILE:
            Age: {context.Age}, BMI: {context.BMI}, Fitness Level: {context.FitnessLevel}
            Goal: {context.Goal}, Injuries: {context.PreviousInjury}
            Equipment: {context.AvailableEquipment}, Duration: {context.WorkoutDuration} mins

            AVAILABLE EXERCISE INVENTORY:
            {exerciseListString}

            STRICT RULE: You must ONLY use exercises from the Available Exercise Inventory above. Do not hallucinate exercises.
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
