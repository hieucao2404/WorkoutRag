CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE "Exercises" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text NOT NULL,
    "Equipment" text NOT NULL,
    "DifficultyLevel" text NOT NULL,
    "MovementPattern" text NOT NULL,
    "ExerciseType" text NOT NULL,
    "MusclesTargeted" text[] NOT NULL,
    "Embedding" vector(768),
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Exercises" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Username" text NOT NULL,
    "Age" integer,
    "WeightKg" numeric,
    "HeighCm" numeric,
    "DailyPosture" text,
    "KnownImbalances" text[] NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "UserDiets" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "DietType" text NOT NULL,
    "Allergies" text[] NOT NULL,
    "MacroPreference" text NOT NULL,
    CONSTRAINT "PK_UserDiets" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserDiets_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserSports" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "SportName" text NOT NULL,
    "PriorityLevel" integer NOT NULL,
    "SeasonStatus" text NOT NULL,
    CONSTRAINT "PK_UserSports" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserSports_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "WorkoutHistories" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "UserPrompt" text NOT NULL,
    "EquipmentFilter" text NOT NULL,
    "RawAiJson" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_WorkoutHistories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_WorkoutHistories_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "WorkoutExercises" (
    "Id" uuid NOT NULL,
    "WorkoutId" uuid NOT NULL,
    "ExerciseId" uuid NOT NULL,
    "RecommendedSets" integer NOT NULL,
    "RecommendedReps" text NOT NULL,
    CONSTRAINT "PK_WorkoutExercises" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_WorkoutExercises_Exercises_ExerciseId" FOREIGN KEY ("ExerciseId") REFERENCES "Exercises" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_WorkoutExercises_WorkoutHistories_WorkoutId" FOREIGN KEY ("WorkoutId") REFERENCES "WorkoutHistories" ("Id") ON DELETE CASCADE
);

CREATE INDEX idx_exercise_embedding_hnsw ON "Exercises" USING hnsw ("Embedding" vector_cosine_ops);

CREATE INDEX idx_exercise_equipment_difficulty ON "Exercises" ("Equipment", "DifficultyLevel");

CREATE INDEX "IX_UserDiets_UserId" ON "UserDiets" ("UserId");

CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");

CREATE INDEX "IX_UserSports_UserId" ON "UserSports" ("UserId");

CREATE INDEX "IX_WorkoutExercises_ExerciseId" ON "WorkoutExercises" ("ExerciseId");

CREATE INDEX "IX_WorkoutExercises_WorkoutId" ON "WorkoutExercises" ("WorkoutId");

CREATE INDEX "IX_WorkoutHistories_UserId" ON "WorkoutHistories" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260531130338_InitialCreate', '10.0.8');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" RENAME COLUMN "HeighCm" TO "HeightCm";

ALTER TABLE "Users" ADD "ComputedBiomechanicalNeeds" text[] NOT NULL;

CREATE TABLE "UserLifestyleProfile" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Occupation" integer NOT NULL,
    "Movement_SittingHoursPerDay" integer NOT NULL,
    "Movement_StandingHoursPerDay" integer NOT NULL,
    "Movement_WalkingHoursPerDay" integer NOT NULL,
    "Movement_PhysicalLaborHoursPerDay" integer NOT NULL,
    "Stressors_RepetitiveLifting" boolean NOT NULL,
    "Stressors_RepetitiveBending" boolean NOT NULL,
    "Stressors_OverheadWork" boolean NOT NULL,
    "Stressors_OneSidedLoadCarrying" boolean NOT NULL,
    "Stressors_ProlongedSitting" boolean NOT NULL,
    "Stressors_ProlongedStanding" boolean NOT NULL,
    "Stressors_FrequentStairClimbing" boolean NOT NULL,
    "Recovery_AverageSleepHours" integer NOT NULL,
    "Recovery_StressLevel" integer NOT NULL,
    "Recovery_FeelsFatiguedAfterWork" boolean NOT NULL,
    "Recovery_HasChronicPain" boolean NOT NULL,
    "Habits_DailyStepCount" integer NOT NULL,
    "Habits_StretchesRegularly" boolean NOT NULL,
    "Habits_TakesMovementBreaks" boolean NOT NULL,
    "Habits_WaterIntakeLiters" numeric NOT NULL,
    "Habits_ScreenTimeHours" integer NOT NULL,
    "Pain_NeckPain" boolean NOT NULL,
    "Pain_ShoulderPain" boolean NOT NULL,
    "Pain_UpperBackPain" boolean NOT NULL,
    "Pain_LowerBackPain" boolean NOT NULL,
    "Pain_KneePain" boolean NOT NULL,
    "Pain_AnklePain" boolean NOT NULL,
    CONSTRAINT "PK_UserLifestyleProfile" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserLifestyleProfile_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_UserLifestyleProfile_UserId" ON "UserLifestyleProfile" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260604020733_ImplementDomainDrivenProfiles', '10.0.8');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" ADD "AthleticLevel" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260604073814_CleanUserSchema', '10.0.8');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" DROP COLUMN "DailyPosture";

ALTER TABLE "Users" DROP COLUMN "KnownImbalances";

ALTER TABLE "Users" ADD "Email" text NOT NULL DEFAULT '';

ALTER TABLE "Users" ADD "PasswordHash" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260604085612_AddAuthenticationFields', '10.0.8');

COMMIT;

START TRANSACTION;
CREATE TABLE "NutritionPlan" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "UserGoal" text NOT NULL,
    "DietaryRestrictions" text NOT NULL,
    "DailyCalories" integer NOT NULL,
    "ProteinsGrams" integer NOT NULL,
    "CarbsGrams" integer NOT NULL,
    "FatGrams" integer NOT NULL,
    "MealPlanJson" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_NutritionPlan" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_NutritionPlan_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_NutritionPlan_UserId" ON "NutritionPlan" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260625020946_AddNutritionPlan', '10.0.8');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" ADD "Gender" text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260706033616_AddGenderToUser', '10.0.8');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" ADD "Role" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260717154005_AddRoleToUser', '10.0.8');

COMMIT;

