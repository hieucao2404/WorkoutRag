using WorkoutRag.Models;

namespace WorkoutRag.Services;

public static class BiomechanicalAnalyzer
{
    public static List<string> CalculateNeeds(UserLifestyleProfile profile)
    {
        var needs = new List<string>();

        /// ---------------------------------------------------------
        // 1. MOVEMENT & POSTURE DYNAMICS (The "Grind" Analysis)
        // ---------------------------------------------------------
        if (profile.Movement.SittingHoursPerDay >= 6)
        {
            if (profile.Habits.DailyStepCount < 5000 && !profile.Habits.TakesMovementBreaks)
            {
                needs.Add(
                    "[Mobility] Severe Sedentary Risk: Prioritize aggressive hip flexor mobilization and glute activation."
                );
                needs.Add(
                    "[Programming] Build baseline aerobic capacity before prescribing heavy anaerobic loading."
                );
            }
            else
            {
                needs.Add(
                    "[Mobility] Active Sitter: Include standard hip mobilization and maintain posterior chain strength."
                );
            }
        }

        // The Standing Toll (Nurses / Retail / Factory)
        if (profile.Movement.StandingHoursPerDay >= 6)
        {
            needs.Add(
                "[Recovery] High Plantar/Calf Fatigue: Prescribe lower-leg soft tissue work and elevate feet post-workout."
            );
            needs.Add(
                "[Programming] Avoid unnecessary standing exercises (e.g., swap standing overhead press for seated)."
            );
        }

        // The Labor Toll (Construction / Warehouse)
        if (profile.Movement.PhysicalLaborHoursPerDay >= 4)
        {
            needs.Add(
                "[Programming] High Daily Mechanical Tension: Reduce total workout volume by 20%. Emphasize joint decompression."
            );
        }

        // ---------------------------------------------------------
        // 2. OCCUPATIONAL STRESSORS (Repetitive Micro-Trauma)
        // ---------------------------------------------------------

        if (profile.Stressors.OneSidedLoadCarrying)
        {
            needs.Add(
                "[Corrective] Asymmetrical Load Shift: Prescribe anti-rotation core work (e.g., Pallof Presses, Suitcase Carries) to balance the Quadratus Lumborum (QL)."
            );
        }

        if (profile.Stressors.RepetitiveBending || profile.Stressors.RepetitiveLifting)
        {
            needs.Add(
                "[Corrective] Lumbar Erector Overload: Prescribe spinal decompression, cat-cow mobility, and strictly enforce neutral-spine hinging."
            );
        }

        if (profile.Stressors.OverheadWork)
        {
            needs.Add(
                "[Corrective] Shoulder Impingement Risk: Avoid heavy vertical pressing. Focus on thoracic extension and lower trapezius stability."
            );
        }

        if (profile.Stressors.FrequentStairClimbing)
        {
            needs.Add(
                "[Programming] Patellar Tendon Stress: Prioritize hamstring/glute dominance in leg training to offload the anterior knee."
            );
        }

        // ---------------------------------------------------------
        // 3. DAILY HABITS (The Hidden Factors)
        // ---------------------------------------------------------

        if (profile.Habits.ScreenTimeHours >= 6)
        {
            needs.Add(
                "[Corrective] Upper Crossed Syndrome Risk: Integrate deep cervical flexor strengthening, band pull-aparts, and chest stretching."
            );
        }

        if (
            profile.Habits.WaterIntakeLiters < 2.0m
            && profile.Movement.PhysicalLaborHoursPerDay > 0
        )
        {
            needs.Add(
                "[Recovery] Dehydration Risk: Fascial tissues may be stiff. Enforce a longer, slower warm-up sequence."
            );
        }

        // ---------------------------------------------------------
        // 4. RECOVERY PROFILE (The Central Nervous System Governor)
        // ---------------------------------------------------------

        bool isChronicallyFatigued =
            profile.Recovery.AverageSleepHours < 6 || profile.Recovery.FeelsFatiguedAfterWork;
        bool isHighStress = profile.Recovery.StressLevel >= 8;

        if (isChronicallyFatigued || isHighStress)
        {
            needs.Add(
                "[Governor] CNS Burnout Detected: Cap workout intensity at RPE 7. Emphasize active recovery, parasympathetic breathing, and avoid going to muscular failure."
            );
        }

        // ---------------------------------------------------------
        // 5. CLINICAL PAIN CONSTRAINTS (The Hard Red Flags)
        // ---------------------------------------------------------

        if (profile.Pain.LowerBackPain)
            needs.Add(
                "[RED FLAG] Lower Back Pain: ZERO heavy axial loading allowed. No Barbell Back Squats or conventional Deadlifts. Substitute with supported or unilateral leg work."
            );

        if (profile.Pain.NeckPain)
            needs.Add(
                "[RED FLAG] Neck Pain: Avoid placing loads on the cervical spine (e.g., heavy barbell back squats). No heavy shrugs."
            );

        if (profile.Pain.ShoulderPain)
            needs.Add(
                "[RED FLAG] Shoulder Pain: Restrict overhead pressing. Utilize neutral-grip pressing or push-up variations only."
            );

        if (profile.Pain.KneePain)
            needs.Add(
                "[RED FLAG] Knee Pain: Remove high-impact plyometrics (no box jumps). Favor low-impact, controlled tempo lower-body movements."
            );

        if (profile.Pain.AnklePain)
            needs.Add(
                "[RED FLAG] Ankle Pain: Limit exercises requiring extreme ankle dorsiflexion. Substitute standing calf raises with seated variations."
            );

        return needs.Distinct().ToList();
    }
}
