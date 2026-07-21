# WorkoutRAG Experiment

This folder contains the 20-profile evaluation dataset, the scoring rubric, and a runner for comparing the LLM-only and RAG pipelines.

## Requirements

1. PostgreSQL with pgvector is running and reachable by the application.
2. Ollama is running locally.
3. The required models are installed:

```bash
ollama pull phi3:mini
ollama pull nomic-embed-text
```

4. The WorkoutRAG API is running, for example:

```bash
dotnet run
```

## Run

```bash
python3 experiments/run_experiment.py run --base-url http://localhost:5000
```

If the API uses a different port, change `--base-url`.

The runner writes:

- `experiments/results/raw_outputs.json`
- `experiments/results/blinded_outputs.json`
- `experiments/results/evaluation_sheet.csv`
- `experiments/results/label_key.csv`

The `evaluation_sheet.csv` file contains empty score columns for manual evaluation. After scoring, run:

```bash
python3 experiments/run_experiment.py summarize --scores experiments/results/evaluation_sheet.csv
```

The summary command writes:

- `experiments/results/summary.json`
- `experiments/results/summary.csv`

Do not fill scores randomly. If a generated output is invalid or unusable, score it according to the rubric and record the issue in the notes column.

## Completed Run

The completed July 21, 2026 run is stored in `experiments/results/`. The final paper uses:

- `label_key_clean.csv` for the final condition/label mapping.
- `evaluation_scored.csv` for the single-evaluator rubric scores.
- `summary.csv` and `summary.json` for average quality scores and response times.
- `failed_attempts.csv` to preserve one earlier timeout attempt that was excluded from final scoring.
