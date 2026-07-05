using System;

namespace WorkoutRag.Models;

public class NutritionPlan
{
    public Guid Id {get;set;}
    public Guid UserId{get;set;}

    public string UserGoal{get;set;} = default;
    public string DietaryRestrictions{get;set;} = string.Empty;

    //Core Daily Targets
    public int DailyCalories {get;set;}
    public int ProteinsGrams{get;set;}
    public int CarbsGrams{get;set;}
    public int FatGrams{get;set;}

    // The Json output from the AI containing the actual meals
    public string MealPlanJson{get;set;} = default!;
    public DateTime CreatedAt{get;set;} = DateTime.UtcNow;

    //Navigation Property
    public User user{get;set;} = default!;
}