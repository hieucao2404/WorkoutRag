using WorkoutRag.Models;

namespace WorkoutRag.DTO;

public class AdminUserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string AthleticLevel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

public class AdminExerciseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public string MovementPattern { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty;
    public List<string> MusclesTargeted { get; set; } = new();
}

public class AdminExerciseResponse : AdminExerciseRequest
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminWorkoutResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public string EquipmentFilter { get; set; } = string.Empty;
    public string? RawAiJson { get; set; }
    public DateTime CreatedAt { get; set; }
}
