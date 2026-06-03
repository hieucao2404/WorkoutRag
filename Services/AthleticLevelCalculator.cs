namespace WorkoutRag.Services;

public static class AthleticLevelCalculator
{
    public static string CalculateLevel(
        int pushUps,
        int pullUps,
        int plankSeconds,
        int squatReps,
        int? age = null
    )
    {
        int score = 0;

        // Push-up scoring
        if (pushUps >= 50)
            score += 3;
        else if (pushUps >= 30)
            score += 2;
        else if (pushUps >= 10)
            score += 1;

        // Pull-up scoring
        if (pullUps >= 20)
            score += 3;
        else if (pullUps >= 10)
            score += 2;
        else if (pullUps >= 3)
            score += 1;

        // Plank scoring (seconds)
        if (plankSeconds >= 180)
            score += 3;
        else if (plankSeconds >= 90)
            score += 2;
        else if (plankSeconds >= 30)
            score += 1;

        // Squat scoring (reps in 60 sec)
        if (squatReps >= 60)
            score += 3;
        else if (squatReps >= 40)
            score += 2;
        else if (squatReps >= 20)
            score += 1;

        // Age adjustment (older athletes get slight credit)
        if (age.HasValue && age >= 50)
            score += 1;

        return score switch
        {
            >= 10 => "Elite",
            >= 8 => "Advanced",
            >= 5 => "Intermediate",
            >= 2 => "Beginner",
            _ => "Untrained",
        };
    }
}
