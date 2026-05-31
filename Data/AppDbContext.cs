using Microsoft.EntityFrameworkCore;
using WorkoutRag.Models;

namespace WorkoutRag.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<WorkoutHistory> WorkoutHistories { get; set; }
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
    public DbSet<UserSport> UserSports { get; set; }
    public DbSet<UserDiet> UserDiets { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Enable the pgvector extension in PostgreSQL
        modelBuilder.HasPostgresExtension("vector");

        // User -  Username should be unque
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

        //Exercise - HNSW Index for vector similarity
        modelBuilder.Entity<Exercise>().Property(e => e.Embedding).HasColumnType("vector(768)");

        // HNSW Index for fast vector similarity search
        modelBuilder
            .Entity<Exercise>()
            .HasIndex(e => e.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops")
            .HasName("idx_exercise_embedding_hnsw");

        // Optional: Add a covering index for common queries
        modelBuilder
            .Entity<Exercise>()
            .HasIndex(e => new { e.Equipment, e.DifficultyLevel })
            .HasName("idx_exercise_equipment_difficulty");

        //WorkoutExercise relationships
        modelBuilder
            .Entity<WorkoutExercise>()
            .HasOne(we => we.Workout)
            .WithMany(wh => wh.WorkoutExercises)
            .HasForeignKey(we => we.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<WorkoutExercise>()
            .HasOne(we => we.Exercise)
            .WithMany(e => e.WorkoutExercises)
            .HasForeignKey(we => we.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
