#!/usr/bin/env python3
import argparse
import csv
import json
import random
import statistics
import time
import urllib.error
import urllib.request
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parent
DEFAULT_PROFILES = ROOT / "eval_profiles.json"
DEFAULT_RESULTS = ROOT / "results"
METRICS = [
    "goal_relevance",
    "personalization",
    "equipment_compatibility",
    "safety",
    "workout_completeness",
]

QUALITY_HEADERS = [
    "profile_id",
    "label",
    "goal",
    "available_equipment",
    "previous_injury",
    "notes",
    *METRICS,
]


def request_text(base_url, method, path, payload=None, token=None, timeout=360):
    body = None if payload is None else json.dumps(payload).encode("utf-8")
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(
        base_url.rstrip("/") + path,
        data=body,
        headers=headers,
        method=method,
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout) as response:
            return response.read().decode("utf-8")
    except urllib.error.HTTPError as exc:
        detail = exc.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"{method} {path} failed: HTTP {exc.code}: {detail}") from exc


def request_json(base_url, method, path, payload=None, token=None):
    data = request_text(base_url, method, path, payload, token)
    if not data:
        return {}
    return json.loads(data)


def ensure_user(base_url, profile, password):
    username = profile["benchmark"]["username"]
    email = f"{username}@example.invalid"
    register_payload = {"username": username, "email": email, "password": password}
    try:
        request_json(base_url, "POST", "/api/auth/register", register_payload)
    except RuntimeError as exc:
        if "already exists" not in str(exc):
            raise

    login = request_json(
        base_url,
        "POST",
        "/api/auth/login",
        {"username": username, "email": email, "password": password},
    )
    return login["token"], login["userId"]


def build_workout_request(profile, use_rag):
    benchmark = profile["benchmark"]
    return {
        "useRag": use_rag,
        "age": benchmark["age"],
        "height": benchmark["heightCm"],
        "weight": benchmark["weightKg"],
        "bmi": round(benchmark["weightKg"] / ((benchmark["heightCm"] / 100) ** 2), 1),
        "activityLevel": profile["activityLevel"],
        "mentalHealth": profile["mentalHealth"],
        "exerciseFrequency": profile["exerciseFrequency"],
        "goal": profile["goal"],
        "fitnessLevel": "",
        "availableEquipment": profile["availableEquipment"],
        "workoutDuration": profile["requestedDuration"],
        "previousInjury": profile["previousInjury"],
        "additionalRequirements": profile["additionalRequirements"],
    }


def run_generation(base_url, profile, token, condition, timeout):
    payload = build_workout_request(profile, use_rag=(condition == "WorkoutRAG"))
    start = time.perf_counter()
    try:
        output_text = request_text(
            base_url,
            "POST",
            "/api/workout/generate",
            payload,
            token=token,
            timeout=timeout,
        )
        elapsed = time.perf_counter() - start
        error = ""
        try:
            output = json.loads(output_text)
            parse_error = ""
        except json.JSONDecodeError as exc:
            output = output_text
            parse_error = str(exc)
    except Exception as exc:
        elapsed = time.perf_counter() - start
        output = ""
        parse_error = ""
        error = str(exc)
    return {
        "condition": condition,
        "response_time_seconds": elapsed,
        "output": output,
        "parse_error": parse_error,
        "error": error,
    }


def run(args):
    profiles = json.loads(Path(args.profiles).read_text())
    results_dir = Path(args.results_dir)
    results_dir.mkdir(parents=True, exist_ok=True)

    raw_path = results_dir / "raw_outputs.json"
    blinded_path = results_dir / "blinded_outputs.json"
    label_key_path = results_dir / "label_key.csv"
    evaluation_path = results_dir / "evaluation_sheet.csv"

    raw_records = read_json_or_default(raw_path, [])
    blinded_records = read_json_or_default(blinded_path, [])
    label_rows = read_csv_or_default(label_key_path)
    evaluation_rows = read_csv_or_default(evaluation_path)
    completed = {
        (row["profile_id"], row["condition"])
        for row in label_rows
        if row.get("error", "") == ""
    }
    randomizer = random.Random(args.seed)

    for profile in profiles:
        token, _ = ensure_user(args.base_url, profile, args.password)
        request_json(args.base_url, "POST", "/api/onboarding/benchmark", profile["benchmark"], token=token)
        request_json(args.base_url, "POST", "/api/onboarding/lifestyle", profile["lifestyle"], token=token)

        for condition in ["LLM-only", "WorkoutRAG"]:
            if (profile["id"], condition) in completed and not args.rerun_completed:
                continue
            result = run_generation(args.base_url, profile, token, condition, args.timeout)
            label_map = condition_label_map(profile["id"], randomizer)
            label = label_map[condition]
            raw_records.append({"profileId": profile["id"], "profile": profile, "results": [result]})
            blinded_records.append(
                {
                    "profileId": profile["id"],
                    "goal": profile["goal"],
                    "availableEquipment": profile["availableEquipment"],
                    "requestedDuration": profile["requestedDuration"],
                    "previousInjury": profile["previousInjury"],
                    "label": label,
                    "output": result["output"],
                    "error": result["error"],
                }
            )
            label_rows.append(
                {
                    "profile_id": profile["id"],
                    "label": label,
                    "condition": result["condition"],
                    "response_time_seconds": f"{result['response_time_seconds']:.3f}",
                    "error": result["error"],
                    "created_at": datetime.now(timezone.utc).isoformat(),
                }
            )
            row = {
                "profile_id": profile["id"],
                "label": label,
                "goal": profile["goal"],
                "available_equipment": profile["availableEquipment"],
                "previous_injury": profile["previousInjury"],
                "notes": "",
            }
            for metric in METRICS:
                row[metric] = ""
            if result["error"]:
                row["notes"] = f"Generation failed: {result['error']}"
            evaluation_rows.append(row)

            write_json(raw_path, raw_records)
            write_json(blinded_path, blinded_records)
            write_csv(label_key_path, label_rows)
            write_csv(evaluation_path, evaluation_rows, headers=QUALITY_HEADERS)
    print(f"Wrote experiment artifacts to {results_dir}")


def summarize(args):
    scores_path = Path(args.scores)
    label_key_path = Path(args.label_key)
    results_dir = scores_path.parent
    scores = list(csv.DictReader(scores_path.open(newline="")))
    label_key = {
        (row["profile_id"], row["label"]): row
        for row in csv.DictReader(label_key_path.open(newline=""))
    }

    by_condition = {"LLM-only": {m: [] for m in METRICS}, "WorkoutRAG": {m: [] for m in METRICS}}
    times = {"LLM-only": [], "WorkoutRAG": []}

    for row in scores:
        key = (row["profile_id"], row["label"])
        if key not in label_key:
            continue
        condition = label_key[key]["condition"]
        if label_key[key].get("error", ""):
            continue
        time_value = float(label_key[key]["response_time_seconds"])
        times[condition].append(time_value)
        for metric in METRICS:
            value = row.get(metric, "").strip()
            if value:
                by_condition[condition][metric].append(float(value))

    summary = {
        "quality": {},
        "response_time_seconds": {},
    }
    rows = []
    for metric in METRICS:
        llm_avg = mean_or_none(by_condition["LLM-only"][metric])
        rag_avg = mean_or_none(by_condition["WorkoutRAG"][metric])
        diff = None if llm_avg is None or rag_avg is None else round(rag_avg - llm_avg, 3)
        summary["quality"][metric] = {
            "LLM-only": llm_avg,
            "WorkoutRAG": rag_avg,
            "difference": diff,
        }
        rows.append({"metric": metric, "LLM-only": llm_avg, "WorkoutRAG": rag_avg, "difference": diff})

    for condition, values in times.items():
        summary["response_time_seconds"][condition] = {
            "mean": mean_or_none(values),
            "min": min(values) if values else None,
            "max": max(values) if values else None,
        }

    write_json(results_dir / "summary.json", summary)
    write_csv(results_dir / "summary.csv", rows)
    print(json.dumps(summary, indent=2))


def mean_or_none(values):
    return round(statistics.mean(values), 3) if values else None


def condition_label_map(profile_id, randomizer):
    state = randomizer.getstate()
    randomizer.seed(profile_id)
    labels = ["Output A", "Output B"]
    randomizer.shuffle(labels)
    randomizer.setstate(state)
    return {"LLM-only": labels[0], "WorkoutRAG": labels[1]}


def read_json_or_default(path, default):
    if not path.exists():
        return default
    return json.loads(path.read_text())


def read_csv_or_default(path):
    if not path.exists():
        return []
    with path.open(newline="") as handle:
        return list(csv.DictReader(handle))


def write_json(path, data):
    path.write_text(json.dumps(data, indent=2) + "\n")


def write_csv(path, rows, headers=None):
    if not rows:
        return
    with path.open("w", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=headers or list(rows[0].keys()), extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)


def main():
    parser = argparse.ArgumentParser(description="Run or summarize the WorkoutRAG evaluation.")
    subparsers = parser.add_subparsers(dest="command", required=True)

    run_parser = subparsers.add_parser("run")
    run_parser.add_argument("--base-url", default="http://localhost:5000")
    run_parser.add_argument("--profiles", default=str(DEFAULT_PROFILES))
    run_parser.add_argument("--results-dir", default=str(DEFAULT_RESULTS))
    run_parser.add_argument("--password", default="WorkoutRagEval123!")
    run_parser.add_argument("--seed", type=int, default=42)
    run_parser.add_argument("--timeout", type=int, default=360)
    run_parser.add_argument("--rerun-completed", action="store_true")
    run_parser.set_defaults(func=run)

    summary_parser = subparsers.add_parser("summarize")
    summary_parser.add_argument("--scores", default=str(DEFAULT_RESULTS / "evaluation_sheet.csv"))
    summary_parser.add_argument("--label-key", default=str(DEFAULT_RESULTS / "label_key.csv"))
    summary_parser.set_defaults(func=summarize)

    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
