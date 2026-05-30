using System;
using System.Collections.Generic;

namespace WorkoutRag.Models;

public class UserDiet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DietType { get; set; } = default!; // "Standard", "Keto", "Vegan", "Paleo"
    public List<string> Allergies { get; set; } = new();
    public string MacroPreference { get; set; } = default!; // "High Carb", "High Protein"
    
    // Navigation properties
    public User User { get; set; } = default!;
}