using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

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
        List<WorkoutRag.Models.Exercise> exercises
    )
    {
        var exerciseList = string.Join("\n", exercises.Select(e => $"- {e.Name}: {e.Description}"));

        var prompt =
            $@"You are an expert personal trainer. 
The user's goal is: '{userGoal}'.
You MUST build a workout using ONLY the following exercises:
{exerciseList}

Provide a 3-set workout plan. Format your response strictly as clean JSON, with no markdown formatting or extra conversational text.
Example format:
{{
    ""workoutName"": ""Explosive Jump Routine"",
    ""exercises"": [
        {{ ""name"": ""Box Jump"", ""sets"": 3, ""reps"": ""8-10"" }}
    ]
}}";

        var request = new
        {
            model = "phi3:mini",
            prompt = prompt,
            stream = false,
        };

        var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
        return result?.Response ?? "{}";
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
}
