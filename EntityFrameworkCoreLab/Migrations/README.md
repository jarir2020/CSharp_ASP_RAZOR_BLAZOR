# EF Core migrations

The `LearningDbContextFactory` supplies the design-time context used by the
EF CLI. Once the .NET SDK and `dotnet-ef` tool are available, create and apply
the first migration from the repository root:

```bash
dotnet ef migrations add InitialCreate \
  --project EntityFrameworkCoreLab \
  --startup-project EntityFrameworkCoreLab

dotnet ef database update \
  --project EntityFrameworkCoreLab \
  --startup-project EntityFrameworkCoreLab
```

The generated migration files belong in this directory. They are source code
that describes how the schema changes over time, so they should be reviewed and
committed. The console demo uses SQLite in-memory with `EnsureCreatedAsync()`
to stay disposable; production deployments should apply reviewed migrations
instead.
