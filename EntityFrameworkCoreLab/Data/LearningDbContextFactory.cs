using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EntityFrameworkCoreLab.Data;

/// <summary>
/// The EF CLI uses this factory when it needs to create a DbContext for
/// migrations without starting the application.
/// </summary>
public sealed class LearningDbContextFactory : IDesignTimeDbContextFactory<LearningDbContext>
{
    public LearningDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<LearningDbContext> options = new DbContextOptionsBuilder<LearningDbContext>()
            .UseSqlite("Data Source=learning-course.db")
            .Options;

        return new LearningDbContext(options);
    }
}
