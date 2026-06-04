using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutRag.Migrations
{
    /// <inheritdoc />
    public partial class ImplementDomainDrivenProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HeighCm",
                table: "Users",
                newName: "HeightCm");

            migrationBuilder.AddColumn<List<string>>(
                name: "ComputedBiomechanicalNeeds",
                table: "Users",
                type: "text[]",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "UserLifestyleProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Occupation = table.Column<int>(type: "integer", nullable: false),
                    Movement_SittingHoursPerDay = table.Column<int>(type: "integer", nullable: false),
                    Movement_StandingHoursPerDay = table.Column<int>(type: "integer", nullable: false),
                    Movement_WalkingHoursPerDay = table.Column<int>(type: "integer", nullable: false),
                    Movement_PhysicalLaborHoursPerDay = table.Column<int>(type: "integer", nullable: false),
                    Stressors_RepetitiveLifting = table.Column<bool>(type: "boolean", nullable: false),
                    Stressors_RepetitiveBending = table.Column<bool>(type: "boolean", nullable: false),
                    Stressors_OverheadWork = table.Column<bool>(type: "boolean", nullable: false),
                    Stressors_OneSidedLoadCarrying = table.Column<bool>(type: "boolean", nullable: false),
                    Stressors_ProlongedSitting = table.Column<bool>(type: "boolean", nullable: false),
                    Stressors_ProlongedStanding = table.Column<bool>(type: "boolean", nullable: false),
                    Stressors_FrequentStairClimbing = table.Column<bool>(type: "boolean", nullable: false),
                    Recovery_AverageSleepHours = table.Column<int>(type: "integer", nullable: false),
                    Recovery_StressLevel = table.Column<int>(type: "integer", nullable: false),
                    Recovery_FeelsFatiguedAfterWork = table.Column<bool>(type: "boolean", nullable: false),
                    Recovery_HasChronicPain = table.Column<bool>(type: "boolean", nullable: false),
                    Habits_DailyStepCount = table.Column<int>(type: "integer", nullable: false),
                    Habits_StretchesRegularly = table.Column<bool>(type: "boolean", nullable: false),
                    Habits_TakesMovementBreaks = table.Column<bool>(type: "boolean", nullable: false),
                    Habits_WaterIntakeLiters = table.Column<decimal>(type: "numeric", nullable: false),
                    Habits_ScreenTimeHours = table.Column<int>(type: "integer", nullable: false),
                    Pain_NeckPain = table.Column<bool>(type: "boolean", nullable: false),
                    Pain_ShoulderPain = table.Column<bool>(type: "boolean", nullable: false),
                    Pain_UpperBackPain = table.Column<bool>(type: "boolean", nullable: false),
                    Pain_LowerBackPain = table.Column<bool>(type: "boolean", nullable: false),
                    Pain_KneePain = table.Column<bool>(type: "boolean", nullable: false),
                    Pain_AnklePain = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLifestyleProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLifestyleProfile_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLifestyleProfile_UserId",
                table: "UserLifestyleProfile",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLifestyleProfile");

            migrationBuilder.DropColumn(
                name: "ComputedBiomechanicalNeeds",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "HeightCm",
                table: "Users",
                newName: "HeighCm");
        }
    }
}
