using WorkoutRag.DTO;
using WorkoutRag.Interfaces;

namespace WorkoutRag.Services;

public class RagGenerator : IWorkoutGenerator
{
    private readonly IOllamaService _ollamaService;
    private readonly IWorkoutRetrievalService _retrievalService;

    public RagGenerator(IOllamaService ollamaService, IWorkoutRetrievalService retrievalService)
    {
        _ollamaService = ollamaService;
        _retrievalService = retrievalService;
    }

    public async Task<string> GenerateAsync(WorkoutGenerationContext context)
    {
        // 1. RAG STEP: Retrieve exercises based on the user's goal and equipment
        var retrievedExercises = await _retrievalService.SearchExercisesAsync(
            context.Goal,
            context.AvailableEquipment
        );
        var exerciseListString = string.Join(
            "\n",
            retrievedExercises.Select(e => $"- {e.Name}: {e.Description}")
        );

        var adjustmentSection = context.IsAdjustmentRequest
            ? $@"
            WORKOUT MODIFICATION MODE:
            The user rejected the previous workout and wants changes.

            ORIGINAL GENERATED WORKOUT JSON:
            {context.PreviousWorkoutJson}

            USER FEEDBACK:
            {context.UserFeedback}

            MODIFICATION INSTRUCTIONS:
            - Modify the previous workout according to the user's feedback.
            - Keep the original goal and available equipment in mind.
            - Do not ignore the user's feedback.
            - Keep useful exercises from the previous workout if they still match the feedback.
            - Replace exercises only when needed.
            "
            : @"
            NEW WORKOUT MODE:
            Generate a new workout plan based on the user profile, goal, equipment, and available exercise inventory.
            ";

        // 2. Prompt Generation
        var prompt =
            $@"You are an elite clinical strength coach. Design a safe workout plan.
            
            USER PROFILE:
            Age: {context.Age}, Fitness Level: {context.FitnessLevel}
            Goal: {context.Goal}, Injuries: {context.PreviousInjury}
            Equipment: {context.AvailableEquipment}, Duration: {context.WorkoutDuration} mins

            AVAILABLE EXERCISE INVENTORY:
            {exerciseListString}

            {adjustmentSection}

            STRICT RULE:
            1. You must ONLY use exercises from the Available Exercise Inventory above.
            2. Do not hallucinate exercises.
            3. If this is modification mode, follow the user's feedback carefully.
            4. Return pure JSON only. No markdown, no explanation.
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
