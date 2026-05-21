

```markdown
# Local Workout RAG System 🏋

This project is a fully local, privacy-first Retrieval-Augmented Generation (RAG) console application built with **.NET 10**, **Semantic Kernel**, and **Ollama**. It takes user fitness goals and available equipment constraints, matches them against an internal PostgreSQL vector database using mathematical embeddings, and generates a structured JSON workout plan.

##  Architecture Overview

The system is designed with a clear separation of concerns across four main components:

1. **The Blueprints (`Models/`)**
   - Contains strongly typed C# contracts (`WorkoutPlan`, `ExerciseRecord`).
   - Ensures strict JSON deserialization for the LLM output and maps vector fields for the database.
2. **The Librarian (`Services/WorkoutRetrievalService.cs`)**
   - Connects to a local Dockerized PostgreSQL instance running `pgvector`.
   - Idempotently seeds the database on startup by converting English exercise descriptions into 768-dimensional mathematical vectors.
   - Executes Hybrid Search: strictly filtering by equipment before calculating Cosine Similarity to find the nearest neighbor math vectors.
3. **The Orchestrator (`Program.cs`)**
   - Bootstraps the Microsoft Semantic Kernel and connects it to the local Ollama server.
   - Manages the CLI user experience.
   - Engineers the strict JSON-enforced prompt, injects the retrieved context, and parses the final LLM output.
4. **The Brain (Ollama)**
   - Runs 100% locally on your physical hardware (no internet required).
   - Uses `nomic-embed-text` for vector calculations.
   - Uses `phi3:mini` for intelligent text and structured JSON generation.

##  Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Ollama](https://ollama.com/) installed locally.
- [Docker Desktop](https://www.docker.com/) (or Docker Engine) for the database.

### 1. Download the AI Models
Before running the application, you must pull the required models into your local Ollama instance. Open your terminal and run:

```bash
ollama pull phi3:mini
ollama pull nomic-embed-text

```

### 2. Start the Vector Database

The application requires a PostgreSQL database with the `pgvector` extension. Spin up a local container using Docker:

```bash
docker run --name local-pgvector \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=my_secure_password \
  -e POSTGRES_DB=workout_rag \
  -p 5433:5432 \
  -d pgvector/pgvector:pg16

```

### 3. Verify Configuration

Ensure your `appsettings.json` is configured correctly (and set to "Copy to Output Directory" automatically via your `.csproj`). The default configuration should match your Docker setup:

```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "ChatModel": "phi3:mini",
    "EmbeddingModel": "nomic-embed-text"
  },
  "Database": {
    "ConnectionString": "Host=localhost;Port=5433;Database=workout_rag;Username=postgres;Password=my_secure_password;"
  }
}

```

### 4. Run the Application

Navigate to the root directory containing `WorkoutRag.csproj` and execute:

```bash
dotnet run

```



