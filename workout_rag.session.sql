SELECT * FROM public."Exercises"

TRUNCATE TABLE "Exercises" RESTART IDENTITY CASCADE;


UPDATE "Users"
SET "Role" = 'Admin'
WHERE "Email" = 'your-email@example.com';

select * from public."Users"

DELETE FROM public."Users"
WHERE "Role" = 'Admin';

INSERT INTO "Users"
(
  "Id",
  "Username",
  "Email",
  "PasswordHash",
  "Role",
  "ComputedBiomechanicalNeeds",
  "AthleticLevel",
  "CreatedAt"
)
VALUES
(
  gen_random_uuid(),
  'admin',
  'admin@example.com',
  '$2b$12$.ucDJi07HBREkMbM22zB0O06SQjuf/.WNs6UlQ5/G3qk1lNwnTF2y',
  'Admin',
  ARRAY[]::text[],
  'Beginner',
  NOW()
);