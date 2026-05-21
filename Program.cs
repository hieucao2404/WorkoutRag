#pragma warning disable SKEXP0070
#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using WorkoutRag.Services;

namespace WorkoutRag;

class Program
{
    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("   LOCAL OLLAMA WORKOUT RAG SYSTEM");
        Console.WriteLine("========================================\n");
        Console.ResetColor();

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string ollamaEndpoint = config["Ollama:Endpoint"] ?? "http://localhost:11434";
        string chatModel = config["Ollama:ChatModel"] ?? "phi3:mini";
        string embeddingModel = config["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
        string dbConn = config["Database:ConnectionString"] ?? "";

        var kernelBuilder = Kernel
            .CreateBuilder()
            .AddOllamaChatCompletion(modelId: chatModel, endpoint: new Uri(ollamaEndpoint))
            .AddOllamaTextEmbeddingGeneration(
                modelId: embeddingModel,
                endpoint: new Uri(ollamaEndpoint)
            );

        var kernel = kernelBuilder.Build();
        var embeddingService =
            kernel.Services.GetRequiredService<Microsoft.SemanticKernel.Embeddings.ITextEmbeddingGenerationService>();

        var retrievalService = new WorkoutRetrievalService(embeddingService, dbConn);
        await retrievalService.InitializeAndSeedAsync();

        Console.Write("\n[Input] State your routine goal (e.g., 'build chest mass'): ");
        string goal = Console.ReadLine() ?? "build chest mass";

        Console.Write("[Input] Available equipment gear ('dumbbells', 'barbell', 'bodyweight'): ");
        string equipment = Console.ReadLine() ?? "dumbbells";

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nProcessing Step 1: Matching vectors locally inside pgvector...");
        string injectedContext = await retrievalService.SearchExercisesAsync(goal, equipment);

        Console.WriteLine(
            "Processing Step 2: Streaming schema configuration down to Ollama layer..."
        );

        // FIXED: Using $$""" means single braces '{' are treated as normal text,
        // and we use double braces '{{goal}}' to inject variables.
        var prompt = $$"""
            You are a world-class strength programming model. Create an effective training prescription.
            You must select exercises solely from the text dataset below. Do not assume or extrapolate other gear types.

            You MUST respond with ONLY a raw JSON object matching this exact structure:
            {
              "plan_title": "string",
              "workouts": [
                {
                  "title": "string",
                  "exercises": [
                    { "name": "string", "sets": 3, "rep_range": "8-12" }
                  ]
                }
              ]
            }

            Goal Details: {{goal}}
            Injected Database Profile Constraints: 
            {{injectedContext}}
            """;

        var settings = new OllamaPromptExecutionSettings
        {
            Temperature = 0.2f, // Requires explicit float casting
            ExtensionData = new Dictionary<string, object>
            {
                { "format", "json" }, // This forces Ollama JSON mode in newer SK versions
            },
        };

        var response = await kernel.InvokePromptAsync(prompt, new(settings));
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n================ OUTPUT PAYLOAD ================");

        try
        {
            var neatJson = JsonSerializer.Deserialize<JsonElement>(response.ToString());
            Console.WriteLine(
                JsonSerializer.Serialize(
                    neatJson,
                    new JsonSerializerOptions { WriteIndented = true }
                )
            );
        }
        catch (JsonException)
        {
            Console.WriteLine("Raw LLM Output (Failed to parse as strict JSON):");
            Console.WriteLine(response.ToString());
        }

        Console.WriteLine("================================================");
        Console.ResetColor();
    }
}
