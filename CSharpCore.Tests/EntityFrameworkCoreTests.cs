using EntityFrameworkCoreLab.Data;
using EntityFrameworkCoreLab.Models;
using EntityFrameworkCoreLab.Queries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpCore.Tests;

public sealed class EntityFrameworkCoreTests
{
    [Fact]
    public async Task EnsureCreated_applies_seed_data_and_relationships()
    {
        await using SqliteConnection connection = await OpenConnectionAsync();
        await using LearningDbContext db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        CourseQueryService queries = new();

        CourseDetails? details = await queries.GetDetailsAsync(db, 1);

        Assert.NotNull(details);
        Assert.Equal("C# Fundamentals", details.Title);
        Assert.Equal("Beginner", details.Difficulty);
        Assert.Equal(new[] { "Ava" }, details.StudentNames);
    }

    [Fact]
    public async Task LINQ_query_supports_filtering_sorting_projection_and_pagination()
    {
        await using SqliteConnection connection = await OpenConnectionAsync();
        await using LearningDbContext db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        CourseQueryService queries = new();

        IReadOnlyList<CourseSummary> page = await queries.SearchAsync(
            db,
            titleSearch: "Core",
            minimumPrice: 100m,
            page: 1,
            pageSize: 1);

        Assert.Single(page);
        Assert.Equal("ASP.NET Core", page[0].Title);
        Assert.Equal(0, page[0].EnrollmentCount);
    }

    [Fact]
    public async Task Tracking_and_no_tracking_queries_have_different_change_tracker_behavior()
    {
        await using SqliteConnection connection = await OpenConnectionAsync();
        await using LearningDbContext db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        CourseQueryService queries = new();

        Course? tracked = await queries.LoadTrackedAsync(db, 1);
        Course? readOnly = await queries.LoadReadOnlyAsync(db, 2);

        Assert.NotNull(tracked);
        Assert.NotNull(readOnly);
        Assert.Equal(EntityState.Unchanged, db.Entry(tracked).State);
        Assert.Equal(EntityState.Detached, db.Entry(readOnly).State);
    }

    [Fact]
    public async Task Aggregations_joins_and_parameterized_raw_sql_return_expected_rows()
    {
        await using SqliteConnection connection = await OpenConnectionAsync();
        await using LearningDbContext db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        CourseQueryService queries = new();

        decimal averagePrice = await queries.GetAveragePriceAsync(db);
        IReadOnlyList<EnrollmentSummary> enrollments = await queries.JoinEnrollmentsAsync(db);
        IReadOnlyList<CourseSummary> rawSqlCourses = await queries.SearchWithRawSqlAsync(db, 125m);

        Assert.Equal(125m, averagePrice);
        Assert.Contains(enrollments, item => item.StudentName == "Ava" && item.CourseTitle == "C# Fundamentals");
        Assert.Equal(new[] { "Entity Framework Core", "ASP.NET Core" }, rawSqlCourses.Select(course => course.Title));
    }

    [Fact]
    public async Task Transactional_enrollment_and_composite_key_prevent_duplicates()
    {
        await using SqliteConnection connection = await OpenConnectionAsync();
        await using LearningDbContext db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        CourseQueryService queries = new();

        await queries.EnrollStudentAsync(db, studentId: 2, courseId: 1);

        await Assert.ThrowsAsync<EnrollmentConflictException>(
            () => queries.EnrollStudentAsync(db, studentId: 2, courseId: 1));

        Assert.Equal(2, await db.Enrollments.CountAsync(enrollment => enrollment.CourseId == 1));
    }

    private static LearningDbContext CreateContext(SqliteConnection connection)
    {
        DbContextOptions<LearningDbContext> options = new DbContextOptionsBuilder<LearningDbContext>()
            .UseSqlite(connection)
            .Options;

        return new LearningDbContext(options);
    }

    private static async Task<SqliteConnection> OpenConnectionAsync()
    {
        SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }
}
