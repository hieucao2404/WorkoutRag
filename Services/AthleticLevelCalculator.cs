namespace WorkoutRag.Services;

using System;
using System.Collections.Generic;
using WorkoutRag.DTO;

public static class AthleticLevelCalculator
{
    public static FitnessAssessmentResult CalculateAssessment(BenchmarkTestRequest req)
    {
        bool isFemale = req.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase);

        // --- AGE MULTIPLIER ---
        double ageMultiplier = 1.0;
        if (req.Age.HasValue && req.Age > 40)
        {
            ageMultiplier = Math.Max(1.0 - ((req.Age.Value - 40) * 0.015), 0.75);
        }

        // ==========================================
        // 1. STRENGTH SCORE (Upper Body Absolute)
        // ==========================================
        double maxPushUps = (isFemale ? 30.0 : 50.0) * ageMultiplier;
        double maxPullUps = (isFemale ? 10.0 : 20.0) * ageMultiplier;

        double pushupScore = Math.Min(req.PushUpsMax / maxPushUps, 1.0);
        double pullupScore = Math.Min(req.PullUpsMax / maxPullUps, 1.0);

        // 40% Push, 60% Pull (Pull-ups are a stronger indicator of relative strength)
        double strengthScore = (pushupScore * 0.4) + (pullupScore * 0.6);

        // ==========================================
        // 2. POWER SCORE (Lower Body Explosiveness)
        // ==========================================
        double maxBroadJumpCm = (isFemale ? 200.0 : 250.0) * ageMultiplier;
        double powerScore = Math.Min(req.BroadJumpCm / maxBroadJumpCm, 1.0);

        // ==========================================
        // 3. ENDURANCE SCORE (Core, Legs, & Cardio)
        // ==========================================
        double maxPlank = 180.0 * ageMultiplier;
        double maxSquats = 60.0 * ageMultiplier;

        double plankScore = Math.Min(req.PlankHoldSeconds / maxPlank, 1.0);
        double squatScore = Math.Min(req.SquatReps / maxSquats, 1.0);

        // 1Km Run Math:
        // Target: ~4 mins (240s) for Men, ~4.5 mins (270s) for Women
        // Baseline (0% score): ~10 mins (600s) walking pace
        double target1KmTime = isFemale ? 270.0 : 240.0;
        double baseline1KmTime = 600.0;
        double cardioScore = 0.0;

        if (req.OneKmRunTimeSeconds > 0)
        {
            cardioScore = Math.Clamp(
                (baseline1KmTime - req.OneKmRunTimeSeconds) / (baseline1KmTime - target1KmTime),
                0.0,
                1.0
            );
        }

        // Blend: 20% Core, 30% Leg Lactic, 50% VO2 Max
        double enduranceScore = (plankScore * 0.20) + (squatScore * 0.30) + (cardioScore * 0.50);

        // ==========================================
        // 4. MOBILITY SCORE (4-Part Test)
        // ==========================================
        double mobilityScore = 0.0;
        if (req.CanDeepSquat)
            mobilityScore += 0.25; // Hips/Ankles
        if (req.CanTouchToes)
            mobilityScore += 0.25; // Hamstrings/Lumbar
        if (req.CanPerformWallSlide)
            mobilityScore += 0.25; // Thoracic/Shoulder Flexion
        if (req.CanReachHandsBehindBack)
            mobilityScore += 0.25; // Rotator Cuff (Int/Ext Rotation)

        // ==========================================
        // 5. RECOVERY SCORE (Readiness)
        // ==========================================
        double sleepScore = Math.Min(req.AverageSleepHours / 8.0, 1.0);
        double stressScore = Math.Max((10.0 - req.StressLevel) / 9.0, 0.0);

        double recoveryScore = (sleepScore * 0.6) + (stressScore * 0.4);

        // ==========================================
        // FINAL BLENDED SCORE & CLASSIFICATION
        // ==========================================
        double totalWeightedScore =
            (strengthScore * 0.25)
            + (powerScore * 0.20)
            + (enduranceScore * 0.25)
            + (mobilityScore * 0.15)
            + (recoveryScore * 0.15);

        double finalScore100 = Math.Round(totalWeightedScore * 100, 1);

        string level = finalScore100 switch
        {
            >= 97 => "Elite",
            >= 85 => "Advanced",
            >= 70 => "Intermediate",
            >= 50 => "Novice"
            >= 20 => "Beginner",
            _ => "Untrained",
        };

        // ==========================================
        // WEAKNESS / STRENGTH IDENTIFICATION
        // ==========================================
        var weakAreas = new List<string>();
        var strongAreas = new List<string>();

        // Thresholds for tagging
        if (strengthScore < 0.4)
            weakAreas.Add("Upper Body Absolute Strength");
        if (powerScore < 0.4)
            weakAreas.Add("Lower Body Explosive Power");
        if (cardioScore < 0.4)
            weakAreas.Add("Cardiovascular Capacity (VO2 Max)");
        if (mobilityScore < 0.5)
            weakAreas.Add("Joint Mobility & Postural Flexibility");
        if (recoveryScore < 0.5)
            weakAreas.Add("Systemic Recovery (High CNS Fatigue)");

        if (strengthScore > 0.8)
            strongAreas.Add("Upper Body Absolute Strength");
        if (powerScore > 0.8)
            strongAreas.Add("Lower Body Explosive Power");
        if (cardioScore > 0.8)
            strongAreas.Add("Cardiovascular Capacity");
        if (mobilityScore > 0.9)
            strongAreas.Add("Joint Mobility");
        if (recoveryScore > 0.8)
            strongAreas.Add("Systemic Recovery");

        return new FitnessAssessmentResult
        {
            TotalScore = finalScore100,
            Level = level,
            StrengthScore = Math.Round(strengthScore * 100, 1),
            PowerScore = Math.Round(powerScore * 100, 1),
            EnduranceScore = Math.Round(enduranceScore * 100, 1),
            MobilityScore = Math.Round(mobilityScore * 100, 1),
            RecoveryScore = Math.Round(recoveryScore * 100, 1),
            WeakAreas = weakAreas,
            StrongAreas = strongAreas,
        };
    }
}
