# Phase 6: Entity Framework Core

Entity Framework Core (EF Core) is the .NET ORM. It maps C# entity classes to
database tables and translates LINQ expressions into SQL. The
`EntityFrameworkCoreLab` project uses SQLite so the examples are local and
repeatable.

## 1. Laravel to EF Core mapping

| Laravel | EF Core |
| --- | --- |
| Eloquent model | Entity class |
| Model relationship | Navigation property |
| Query Builder | LINQ over `IQueryable<T>` |
| `where()` | `Where(...)` |
| `first()` | `FirstOrDefaultAsync()` |
| `find()` | `FindAsync()` |
| Migration | Migration |
| Seeder | `HasData` or application seeding |
| Repository | Optional service or repository abstraction |

EF Core is more than a collection wrapper. A query over `IQueryable<T>` is an
expression that the provider can translate to SQL. The database performs the
filtering, sorting, joining, and aggregation when the query is executed.

## 2. `DbContext`, `DbSet`, and entities

`DbContext` represents a unit of work with the database. `DbSet<T>` represents
an entity set and is the starting point for queries and changes.

```csharp
public sealed class LearningDbContext : DbContext
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
}
```

The context is configured with `DbContextOptions`, which selects the provider
and connection string. ASP.NET Core applications normally register the context
as scoped so one HTTP request uses one unit of work.

## 3. Keys, constraints, and relationships

The lab models:

- `Course` to `CourseSettings`: one-to-one. `CourseSettings.CourseId` is both
  its primary key and foreign key.
- `Course` to `Enrollment`: one-to-many.
- `Student` to `Enrollment`: one-to-many.
- `Student` to `Course`: many-to-many through `Enrollment`, which also stores
  `EnrolledAtUtc` and `Status` as relationship payload.

The model configuration also defines:

- primary keys;
- a unique index on course slugs;
- a unique index on student email;
- a composite key on `(StudentId, CourseId)` to prevent duplicate enrollment;
- required string lengths and decimal precision;
- cascade behavior for related rows.

Use constraints to protect invariants at the database boundary. Application
validation improves the client experience, but a database constraint remains
the final protection against concurrent writes.

## 4. Tracking and no-tracking queries

By default, EF Core tracks entity instances returned from a query. Tracking is
useful when the entity will be changed and saved:

```csharp
Course? course = await db.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
course!.Title = "Updated title";
await db.SaveChangesAsync();
```

For read-only work, `AsNoTracking()` avoids change-tracker overhead:

```csharp
Course? course = await db.Courses
    .AsNoTracking()
    .FirstOrDefaultAsync(c => c.CourseId == id);
```

Use the smallest behavior needed by the operation. Do not modify a no-tracking
entity and assume `SaveChangesAsync` will discover the change.

## 5. Relationships with `Include` and `ThenInclude`

Navigation properties are not automatically loaded in every configuration.
Explicit eager loading makes the required graph clear:

```csharp
Course? course = await db.Courses
    .Include(c => c.Settings)
    .Include(c => c.Enrollments)
        .ThenInclude(e => e.Student)
    .SingleOrDefaultAsync(c => c.CourseId == id);
```

`Include` loads a related navigation. `ThenInclude` continues from that
navigation. For API responses, a projection with `Select` is often better than
loading a large graph because it retrieves exactly the response fields.

## 6. Queries and projections

The lab demonstrates:

- filtering with `Where`;
- stable sorting with `OrderBy` and `ThenBy`;
- pagination with `Skip` and `Take`;
- aggregation with `Average` and `Count`;
- joins between enrollments, students, and courses;
- projections into `CourseSummary` and `CourseDetails`;
- raw SQL through `FromSqlInterpolated`.

```csharp
var page = await db.Courses
    .AsNoTracking()
    .Where(course => course.Price >= minimumPrice)
    .OrderBy(course => course.Title)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(course => new CourseSummary
    {
        CourseId = course.CourseId,
        Title = course.Title,
        Price = course.Price,
        EnrollmentCount = course.Enrollments.Count()
    })
    .ToListAsync();
```

Keep the query as `IQueryable<T>` until the database should execute it. Methods
such as `ToListAsync`, `SingleAsync`, `CountAsync`, and `AverageAsync` execute
the query. Avoid calling `ToList()` early and then filtering a large table in
application memory.

## 7. Raw SQL and parameters

`FromSqlInterpolated` creates parameters for interpolated values:

```csharp
db.Courses.FromSqlInterpolated(
    $"SELECT * FROM Courses WHERE Price >= {minimumPrice}");
```

Never concatenate user input into SQL. Raw SQL is useful for provider-specific
features or queries that are difficult to express in LINQ, but ordinary LINQ
should remain the default because it is composable and type-checked.

## 8. Transactions and `SaveChangesAsync`

`SaveChangesAsync` writes tracked changes. A transaction groups several reads
and writes into one unit:

```csharp
await using var transaction = await db.Database.BeginTransactionAsync();
// validate related rows, add enrollment, and save
await db.SaveChangesAsync();
await transaction.CommitAsync();
```

The enrollment example checks that the student and course exist, prevents a
duplicate composite key, saves the row, and commits. If an exception occurs
before commit, disposal rolls the transaction back.

## 9. Seeding and migrations

`HasData` adds model-managed seed data. It becomes migration operations when a
migration is generated. The design-time factory lets the EF CLI construct the
context without starting the console application:

```bash
dotnet ef migrations add InitialCreate \
  --project EntityFrameworkCoreLab \
  --startup-project EntityFrameworkCoreLab

dotnet ef database update \
  --project EntityFrameworkCoreLab \
  --startup-project EntityFrameworkCoreLab
```

`EnsureCreatedAsync()` is used only by the disposable demo and tests. It is
useful for temporary databases, but it does not create a migration history and
should not replace reviewed migrations for a production database.

For production migrations, review the generated SQL and migration code,
backup the database, apply changes through the deployment process, and plan
for rollback or a forward-fix when data has already changed.

## Run the phase

From the repository root:

```bash
dotnet run --project EntityFrameworkCoreLab/EntityFrameworkCoreLab.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

The next phase adds ASP.NET Core Identity, JWTs, claims, roles, policies, and
security protections around the API.
