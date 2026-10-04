using EntityFrameworkCoreLab.Data;
using EntityFrameworkCoreLab.Queries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== PHASE 6: ENTITY FRAMEWORK CORE ===");

// SQLite in-memory keeps this demo local and disposable. A real application
// would use a configured connection string and apply migrations at deployment.
await using SqliteConnection connection = new("Data Source=:memory:");
await connection.OpenAsync();

DbContextOptions<LearningDbContext> options = new DbContextOptionsBuilder<LearningDbContext>()
    .UseSqlite(connection)
    .Options;

await using LearningDbContext db = new(options);
await db.Database.EnsureCreatedAsync();

CourseQueryService queries = new();
IReadOnlyList<CourseSummary> courses = await queries.SearchAsync(
    db,
    titleSearch: null,
    minimumPrice: 100m,
    page: 1,
    pageSize: 10);

Console.WriteLine("\n--- Projection, filtering, sorting, and pagination ---");
foreach (CourseSummary course in courses)
{
    Console.WriteLine($"{course.CourseId}: {course.Title} ({course.Price:0.00}) - {course.EnrollmentCount} enrollment(s)");
}

Console.WriteLine("\n--- Include and ThenInclude relationships ---");
CourseDetails? details = await queries.GetDetailsAsync(db, 1);
Console.WriteLine($"{details?.Title} / {details?.Difficulty} / Students: {string.Join(", ", details?.StudentNames ?? Array.Empty<string>())}");

Console.WriteLine("\n--- Aggregation, join, and parameterized raw SQL ---");
Console.WriteLine($"Average price: {await queries.GetAveragePriceAsync(db):0.00}");
IReadOnlyList<EnrollmentSummary> enrollments = await queries.JoinEnrollmentsAsync(db);
Console.WriteLine($"Joined enrollment: {enrollments[0].StudentName} -> {enrollments[0].CourseTitle}");
IReadOnlyList<CourseSummary> rawSqlCourses = await queries.SearchWithRawSqlAsync(db, 125m);
Console.WriteLine($"Raw SQL courses at or above 125: {rawSqlCourses.Count}");

Console.WriteLine("\n--- Transaction and composite-key constraint ---");
await queries.EnrollStudentAsync(db, studentId: 2, courseId: 1);
Console.WriteLine("Bob enrolled in C# Fundamentals.");

try
{
    await queries.EnrollStudentAsync(db, studentId: 2, courseId: 1);
}
catch (EnrollmentConflictException exception)
{
    Console.WriteLine($"Expected conflict: {exception.Message}");
}

Console.WriteLine("\nPHASE 6 COMPLETE");
