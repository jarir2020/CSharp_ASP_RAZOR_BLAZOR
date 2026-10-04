using EntityFrameworkCoreLab.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCoreLab.Data;

public sealed class LearningDbContext : DbContext
{
    public LearningDbContext(DbContextOptions<LearningDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseSettings> CourseSettings => Set<CourseSettings>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(course => course.CourseId);
            entity.HasIndex(course => course.Slug).IsUnique();
            entity.Property(course => course.Slug).HasMaxLength(100).IsRequired();
            entity.Property(course => course.Title).HasMaxLength(200).IsRequired();
            entity.Property(course => course.Price).HasPrecision(18, 2);

            entity.HasMany(course => course.Enrollments)
                .WithOne(enrollment => enrollment.Course)
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CourseSettings>(entity =>
        {
            entity.HasKey(settings => settings.CourseId);
            entity.Property(settings => settings.Difficulty).HasMaxLength(40).IsRequired();

            entity.HasOne(settings => settings.Course)
                .WithOne(course => course.Settings)
                .HasForeignKey<CourseSettings>(settings => settings.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(student => student.StudentId);
            entity.HasIndex(student => student.Email).IsUnique();
            entity.Property(student => student.Name).HasMaxLength(120).IsRequired();
            entity.Property(student => student.Email).HasMaxLength(200).IsRequired();

            entity.HasMany(student => student.Enrollments)
                .WithOne(enrollment => enrollment.Student)
                .HasForeignKey(enrollment => enrollment.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(enrollment => new { enrollment.StudentId, enrollment.CourseId });
            entity.Property(enrollment => enrollment.Status)
                .HasConversion<string>()
                .HasMaxLength(30);
        });

        // HasData is model-managed seed data. It is included in migrations and
        // makes the local demonstration repeatable.
        modelBuilder.Entity<Course>().HasData(
            new Course { CourseId = 1, Slug = "csharp-fundamentals", Title = "C# Fundamentals", Price = 100m },
            new Course { CourseId = 2, Slug = "aspnet-core", Title = "ASP.NET Core", Price = 150m },
            new Course { CourseId = 3, Slug = "ef-core", Title = "Entity Framework Core", Price = 125m });

        modelBuilder.Entity<CourseSettings>().HasData(
            new CourseSettings { CourseId = 1, Difficulty = "Beginner", EstimatedHours = 8 },
            new CourseSettings { CourseId = 2, Difficulty = "Intermediate", EstimatedHours = 12 },
            new CourseSettings { CourseId = 3, Difficulty = "Intermediate", EstimatedHours = 10 });

        modelBuilder.Entity<Student>().HasData(
            new Student { StudentId = 1, Name = "Ava", Email = "ava@example.test" },
            new Student { StudentId = 2, Name = "Bob", Email = "bob@example.test" });

        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                EnrolledAtUtc = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Status = EnrollmentStatus.Active
            },
            new Enrollment
            {
                StudentId = 1,
                CourseId = 2,
                EnrolledAtUtc = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc),
                Status = EnrollmentStatus.Completed
            });
    }
}
