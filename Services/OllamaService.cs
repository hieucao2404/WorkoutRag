using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using WorkoutRag.Models;
using WorkoutRag.DTO;

namespace WorkoutRag.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;


    public OllamaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri(
            _configuration["Ollama:Endpoint"] ?? "http://localhost:11434"
        );
    }

    // Get embeddings for vector search
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var request = new { model = "nomic-embed-text", prompt = text };

        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
        return result?.Embedding ?? Array.Empty<float>();
    }

    // Generate workout plan using LLM
    public async Task<string> GenerateWorkoutPlanAsync(
        string userGoal,
        string equipment,
        List<WorkoutRag.Models.Exercise> exercises,
        User user
    )
    {
        var exerciseList = string.Join("\n", exercises.Select(e => $"- {e.Name}: {e.Description}"));

        // var userGoal = request.Prompt;
        // var equipment = request.Equipment;

        var clinicalConstraints =
            user.ComputedBiomechanicalNeeds != null && user.ComputedBiomechanicalNeeds.Any()
                ? string.Join("\n", user.ComputedBiomechanicalNeeds)
                : "No specific biomechanical constraints detected. Proceed with standard programming.";

        var prompt =
            $@"You are an elite clinical strength and conditioning coach specializing in occupational longevity and athletic performance.
            Design a highly optimized, safe workout session using ONLY the provided exercise inventory. 

            ATHLETE PROFILE:
            - Age: {user.Age?.ToString() ?? "Unknown"} 
            - Weight: {user.WeightKg?.ToString() ?? "Unknown"} kg
            - Current Athletic Level: {user.AthleticLevel}
            - Primary Session Goal: '{userGoal}'
            - Available Equipment: '{equipment}'

            CLINICAL CONSTRAINTS & BIOMECHANICAL NEEDS:
            You MUST obey the following directives based on the user's occupational and physical assessment. Treat [RED FLAG] tags as absolute physical boundaries.
            {clinicalConstraints}

            AVAILABLE EXERCISES:
            {exerciseList}

            STRICT RULES:
            1. Do not invent or hallucinate exercises. Use ONLY the exact names from the Available Exercises list.
            2. If an exercise requires equipment the user does not have, DO NOT include it.
            3. You must output pure JSON. No markdown formatting, no explanations before or after the JSON.

            Your response MUST perfectly match this exact JSON schema. Do not add spaces to the keys. Do not invent new keys like 'duration'.
            {{
                ""workoutName"": ""String"",
                ""exercises"": [
                    {{ 
                        ""name"": ""Exact Exercise Name"", 
                        ""sets"": Number, 
                        ""reps"": ""String (e.g., '8-12' or '60 seconds')""
                    }}
                ]
            }}";

        var requestPayload = new
        {
            model = "phi3:mini",
            prompt = prompt,
            stream = false,
            format = "json",
            options = new { temperature = 0.1 },
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestPayload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("/api/generate", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to connect to local Ollama instance. Status: {response.StatusCode}"
            );
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(jsonResponse);

        // Ollama returns the generated text inside the "response" property
        return document.RootElement.GetProperty("response").GetString() ?? "{}";
    }

    private class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    private class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;
    }

    //Generate nutrion plan
    public async Task<string> GenerateNutritionPlanAsync(string goal, User user, UserDiet userDiet)
    {
        var prompt = $@"You are an elite clinical sports nutritionist.
            Design a highly optimized nutrition protocol.
            
            GOAL: '{goal}'
            DIET TYPE: {userDiet.DietType}
            ALLERGIES: {userDiet.Allergies}
            MACRO PREFERENCE: {userDiet.MacroPreference}
            
            ATHLETE PROFILE: Age {user.Age}, Weight {user.WeightKg}kg
        
        STRICT RULE: you must output pure JSON perfectly matching this exact schema:
        {{
         ""dailyCalories"": Number,
                ""proteinGrams"": Number,
                ""carbsGrams"": Number,
                ""fatGrams"": Number,
                ""meals"": [
                    {{ 
                        ""mealName"": ""String (e.g., Breakfast)"", 
                        ""foods"": [""String (e.g., 3 Whole Eggs)"", ""String (e.g., 50g Oats)""] 
                    }}
                ]
        }}";

        var requestPayload = new
        {
            model = "phi3:mini",
            prompt = prompt,
            stream = false,
            format = "json",
            // We give it 1500 words so it doesn't get cut off
            options = new
            {
                temperature = 0.1,
                num_predict = 1500
            },
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(requestPayload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("/api/generate", content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var document = System.Text.Json.JsonDocument.Parse(jsonResponse);
        return document.RootElement.GetProperty("response").GetString() ?? "{}";
    }

    public async Task<string> SendPromptAsync(string prompt)
    {
        var requestPayload = new
        {
            model = "phi3:mini",
            prompt = prompt,
            stream = false,
            format = "json",
            options = new { temperature = 0.1 }
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(requestPayload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("/api/generate", content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var document = System.Text.Json.JsonDocument.Parse(jsonResponse);

        return document.RootElement.GetProperty("response").GetString() ?? "{}";
    }

}
