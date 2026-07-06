using WorkoutRag.Models;
namespace WorkoutRag.Interfaces;
public interface IWorkoutRetrievalService
{
    Task<List<Exercise>> SearchExercisesAsync(string query, string equipment, int limit = 6);
}