using EntityFrameworkCoreLab.Data;
using EntityFrameworkCoreLab.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCoreLab.Queries;

public sealed class CourseSummary
{
    public int CourseId { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public int EnrollmentCount { get; init; }
}

public sealed class CourseDetails
{
    public int CourseId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Difficulty { get; init; } = string.Empty;

    public IReadOnlyList<string> StudentNames { get; init; } = Array.Empty<string>();
}

public sealed class EnrollmentSummary
{
    public string StudentName { get; init; } = string.Empty;

    public string CourseTitle { get; init; } = string.Empty;

    public EnrollmentStatus Status { get; init; }
}

public sealed class EnrollmentConflictException : Exception
{
    public EnrollmentConflictException(string message)
        : base(message)
    {
    }
}

public sealed class CourseQueryService
{
    public async Task<IReadOnlyList<CourseSummary>> SearchAsync(
        LearningDbContext db,
        string? titleSearch,
        decimal? minimumPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        IQueryable<Course> query = db.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(titleSearch))
        {
            query = query.Where(course => course.Title.Contains(titleSearch));
        }

        if (minimumPrice.HasValue)
        {
            query = query.Where(course => course.Price >= minimumPrice.Value);
        }

        // Select is a projection: the database only needs to return the fields
        // used by the response instead of materializing full entity graphs.
        return await query
            .OrderBy(course => course.Title)
            .ThenBy(course => course.CourseId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(course => new CourseSummary
            {
                CourseId = course.CourseId,
                Slug = course.Slug,
                Title = course.Title,
                Price = course.Price,
                EnrollmentCount = course.Enrollments.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CourseDetails?> GetDetailsAsync(
        LearningDbContext db,
        int courseId,
        CancellationToken cancellationToken = default)
    {
        // Include loads the one-to-one settings and the one-to-many enrollments.
        // ThenInclude follows each enrollment to its Student navigation.
        Course? course = await db.Courses
            .AsNoTracking()
            .Include(item => item.Settings)
            .Include(item => item.Enrollments)
                .ThenInclude(enrollment => enrollment.Student)
            .SingleOrDefaultAsync(item => item.CourseId == courseId, cancellationToken);

        if (course is null)
        {
            return null;
        }

        return new CourseDetails
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Difficulty = course.Settings?.Difficulty ?? "Unknown",
            StudentNames = course.Enrollments
                .OrderBy(enrollment => enrollment.Student.Name)
                .Select(enrollment => enrollment.Student.Name)
                .ToList()
        };
    }

    public Task<Course?> LoadTrackedAsync(
        LearningDbContext db,
        int courseId,
        CancellationToken cancellationToken = default)
    {
        // Tracking is useful when the entity will be changed and saved.
        return db.Courses.FirstOrDefaultAsync(course => course.CourseId == courseId, cancellationToken);
    }

    public Task<Course?> LoadReadOnlyAsync(
        LearningDbContext db,
        int courseId,
        CancellationToken cancellationToken = default)
    {
        // NoTracking avoids change-tracker work for read-only endpoints.
        return db.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.CourseId == courseId, cancellationToken);
    }

    public async Task<decimal> GetAveragePriceAsync(
        LearningDbContext db,
        CancellationToken cancellationToken = default)
    {
        // SQLite has provider-specific limitations around decimal aggregate
        // translation, so calculate the SQL average as double and convert the
        // result back to the domain money type at the application boundary.
        double average = await db.Courses
            .AsNoTracking()
            .AverageAsync(course => (double)course.Price, cancellationToken);

        return (decimal)average;
    }

    public async Task<IReadOnlyList<EnrollmentSummary>> JoinEnrollmentsAsync(
        LearningDbContext db,
        CancellationToken cancellationToken = default)
    {
        // This LINQ join is translated to SQL and combines related tables
        // without loading every entity into memory first.
        return await db.Enrollments
            .AsNoTracking()
            .Join(db.Students, enrollment => enrollment.StudentId, student => student.StudentId,
                (enrollment, student) => new { enrollment, student })
            .Join(db.Courses, item => item.enrollment.CourseId, course => course.CourseId,
                (item, course) => new EnrollmentSummary
                {
                    StudentName = item.student.Name,
                    CourseTitle = course.Title,
                    Status = item.enrollment.Status
                })
            .OrderBy(summary => summary.StudentName)
            .ThenBy(summary => summary.CourseTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseSummary>> SearchWithRawSqlAsync(
        LearningDbContext db,
        decimal minimumPrice,
        CancellationToken cancellationToken = default)
    {
        // FromSqlInterpolated parameterizes the value. Do not concatenate raw
        // user input into SQL strings.
        return await db.Courses
            .FromSqlInterpolated($"SELECT * FROM Courses WHERE Price >= {minimumPrice}")
            .AsNoTracking()
            .Select(course => new CourseSummary
            {
                CourseId = course.CourseId,
                Slug = course.Slug,
                Title = course.Title,
                Price = course.Price,
                EnrollmentCount = course.Enrollments.Count()
            })
            .OrderBy(summary => summary.Price)
            .ToListAsync(cancellationToken);
    }

    public async Task EnrollStudentAsync(
        LearningDbContext db,
        int studentId,
        int courseId,
        CancellationToken cancellationToken = default)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await db.Database.BeginTransactionAsync(cancellationToken);

        bool studentExists = await db.Students.AnyAsync(student => student.StudentId == studentId, cancellationToken);
        bool courseExists = await db.Courses.AnyAsync(course => course.CourseId == courseId, cancellationToken);

        if (!studentExists || !courseExists)
        {
            throw new KeyNotFoundException("The student or course does not exist.");
        }

        bool alreadyEnrolled = await db.Enrollments.AnyAsync(
            enrollment => enrollment.StudentId == studentId && enrollment.CourseId == courseId,
            cancellationToken);

        if (alreadyEnrolled)
        {
            throw new EnrollmentConflictException("The student is already enrolled in this course.");
        }

        db.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAtUtc = DateTime.UtcNow,
            Status = EnrollmentStatus.Active
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
