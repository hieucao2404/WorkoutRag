using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WorkoutRag.Models;

// The complex types (Add these to the same file or their own files)
public enum OccupationType
{
    DeskWorker,
    DeliveryDriver,
    ConstructionWorker,
    FactoryWorker,
    RetailWorker,
    HealthcareWorker,
    Student,
    Other,
}

[Owned]
public class DailyMovementProfile
{
    public int SittingHoursPerDay { get; set; }
    public int StandingHoursPerDay { get; set; }
    public int WalkingHoursPerDay { get; set; }
    public int PhysicalLaborHoursPerDay { get; set; }
}

[Owned]
public class OccupationalStressProfile
{
    public bool RepetitiveLifting { get; set; }
    public bool RepetitiveBending { get; set; }
    public bool OverheadWork { get; set; }
    public bool OneSidedLoadCarrying { get; set; }
    public bool ProlongedSitting { get; set; }
    public bool ProlongedStanding { get; set; }
    public bool FrequentStairClimbing { get; set; } // <--- Add this missing line!
}

[Owned]
public class RecoveryProfile
{
    public int AverageSleepHours { get; set; }
    public int StressLevel { get; set; }
    public bool FeelsFatiguedAfterWork { get; set; }
    public bool HasChronicPain { get; set; }
}

[Owned]
public class DailyHabitProfile
{
    public int DailyStepCount { get; set; }
    public bool StretchesRegularly { get; set; }
    public bool TakesMovementBreaks { get; set; }
    public decimal WaterIntakeLiters { get; set; }
    public int ScreenTimeHours { get; set; }
}

[Owned]
public class PainAssessment
{
    public bool NeckPain { get; set; }
    public bool ShoulderPain { get; set; }
    public bool UpperBackPain { get; set; }
    public bool LowerBackPain { get; set; }
    public bool KneePain { get; set; }
    public bool AnklePain { get; set; }
}

public class UserLifestyleProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OccupationType Occupation { get; set; }
    public DailyMovementProfile Movement { get; set; } = new();
    public OccupationalStressProfile Stressors { get; set; } = new();
    public RecoveryProfile Recovery { get; set; } = new();
    public DailyHabitProfile Habits { get; set; } = new();
    public PainAssessment Pain { get; set; } = new();
    public User User { get; set; } = null!;
}
