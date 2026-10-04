# C#, ASP.NET Core, Razor, and Blazor learning track

This repository follows `plan.md` as a theory-first, incremental course for a
developer moving from backend development into the .NET ecosystem. Each phase
contains runnable code, explanatory comments, tests, and a short theory guide.

## Phase 1: C# Fundamentals

Phase 1 covers variables and types, operators, conditions, loops, arrays,
collections, classes, structs, properties, constructors, interfaces,
inheritance, composition, and generics.

Run the lesson:

```bash
dotnet run --project CSharpCore/CSharpCore.csproj
```

Run the tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE1_THEORY_QUICK_GUIDE.md`](guides/PHASE1_THEORY_QUICK_GUIDE.md).

## Phase 2: Modern C#

Phase 2 covers Queue, Stack, collection interfaces, LINQ, delegates, lambdas,
events, custom exceptions, nullable reference and value types, async/await,
cancellation, and CPU-bound parallel work.

Run the lessons and tests with the same commands:

```bash
dotnet run --project CSharpCore/CSharpCore.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE2_THEORY_QUICK_GUIDE.md`](guides/PHASE2_THEORY_QUICK_GUIDE.md).

## Phase 3: .NET Fundamentals

Phase 3 covers the CLR, runtime, SDK, assemblies, DLLs, the Base Class Library,
Garbage Collection, .NET CLI commands, project structure, NuGet, and dependency
injection with transient, scoped, and singleton lifetimes.

Run the lessons and tests with the same commands:

```bash
dotnet run --project CSharpCore/CSharpCore.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE3_THEORY_QUICK_GUIDE.md`](guides/PHASE3_THEORY_QUICK_GUIDE.md).

## Phase 4: ASP.NET Core Fundamentals

Phase 4 introduces HTTP requests and responses, `Program.cs`, configuration,
environments, logging, middleware, routing, endpoints, and exception handling.
The runnable web project is in `AspNetCoreFundamentals/`.

Run the web application:

```bash
dotnet run --project AspNetCoreFundamentals/AspNetCoreFundamentals.csproj
```

Run all tests, including HTTP integration tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE4_THEORY_QUICK_GUIDE.md`](guides/PHASE4_THEORY_QUICK_GUIDE.md).

## Phase 5: ASP.NET Core Web API

Phase 5 adds a controller-based product API with model binding, request and
response DTOs, DataAnnotations, custom validation, JSON options, and consistent
ProblemDetails responses. It uses an in-memory catalog until EF Core arrives
in Phase 6.

Run the API:

```bash
dotnet run --project AspNetCoreWebApi/AspNetCoreWebApi.csproj
```

Run all tests, including API integration tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE5_THEORY_QUICK_GUIDE.md`](guides/PHASE5_THEORY_QUICK_GUIDE.md).

## Phase 6: Entity Framework Core

Phase 6 adds a SQLite EF Core lab with `DbContext`, `DbSet`, one-to-one,
one-to-many, and many-to-many relationships, tracking, no-tracking queries,
LINQ projections, pagination, joins, raw SQL parameters, transactions, and
seed data.

Run the lab:

```bash
dotnet run --project EntityFrameworkCoreLab/EntityFrameworkCoreLab.csproj
```

Run all tests, including EF Core integration tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE6_THEORY_QUICK_GUIDE.md`](guides/PHASE6_THEORY_QUICK_GUIDE.md).

## Phase 7: Authentication and Security

Phase 7 adds a separate secure API with ASP.NET Core Identity, password
hashing, JWT bearer authentication, roles, claims, policies, CORS, rate
limiting, HTTPS redirection, security headers, and protected endpoints.

Run the secure API:

```bash
dotnet run --project SecureAspNetCoreApi/SecureAspNetCoreApi.csproj
```

Run all tests, including authentication and authorization integration tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE7_THEORY_QUICK_GUIDE.md`](guides/PHASE7_THEORY_QUICK_GUIDE.md).

## Phase 8: Razor and Razor Pages

Phase 8 adds a server-rendered `RazorPagesLab` application. It demonstrates
Razor expressions, conditions, loops, layouts, partial views, Tag Helpers,
model binding, DataAnnotations validation, antiforgery protection, and the
`OnGet`/`OnPost` PageModel handlers.

Run the Razor Pages application:

```bash
dotnet run --project RazorPagesLab/RazorPagesLab.csproj
```

Run all tests, including Razor Pages integration tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE8_THEORY_QUICK_GUIDE.md`](guides/PHASE8_THEORY_QUICK_GUIDE.md).

## Phase 9: ASP.NET Core MVC

Phase 9 adds `MvcLab`, a controller-based application that demonstrates
controllers, actions, domain models, view models, Razor views, layouts,
partials, conventional routing, attribute routing, model binding, validation,
antiforgery protection, and action filters.

Run the MVC application:

```bash
dotnet run --project MvcLab/MvcLab.csproj
```

Run all tests, including MVC integration tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE9_THEORY_QUICK_GUIDE.md`](guides/PHASE9_THEORY_QUICK_GUIDE.md).

## Phase 10: Blazor fundamentals

Phase 10 adds `BlazorLab`, a modern Blazor Web App using Interactive Server
rendering. It demonstrates components, routing, parameters, `EventCallback`,
event handling, data binding, `EditForm`, validation, layouts, dependency
injection, scoped state, and component lifecycle methods.

Run the Blazor application:

```bash
dotnet run --project BlazorLab/BlazorLab.csproj
```

Run all tests, including Blazor prerendering integration tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE10_THEORY_QUICK_GUIDE.md`](guides/PHASE10_THEORY_QUICK_GUIDE.md).

## Phase 11: JavaScript interop

Phase 11 adds a JavaScript module to `BlazorLab`. It demonstrates calling
JavaScript from C#, browser APIs, ES modules, DOM focus, `localStorage`, the
clipboard API, and JavaScript callbacks into a .NET object through
`DotNetObjectReference`.

Run the Blazor application:

```bash
dotnet run --project BlazorLab/BlazorLab.csproj
```

Open `/interop` for the interactive interop lesson. Run all tests with:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE11_THEORY_QUICK_GUIDE.md`](guides/PHASE11_THEORY_QUICK_GUIDE.md).

## Phase 12: Advanced ASP.NET Core

Phase 12 adds `AdvancedAspNetCoreLab`, a minimal API that demonstrates typed
configuration and options validation, keyed and open-generic DI, factory
resolution, structured logging, memory and distributed caching, output and
response caching, and a bounded background task queue processed by a hosted
service.

Run the advanced lab:

```bash
dotnet run --project AdvancedAspNetCoreLab/AdvancedAspNetCoreLab.csproj
```

Useful endpoints include `/api/config`, `/api/di`, `/api/cache/memory`,
`/api/cache/output`, `/api/cache/response`, and `/api/jobs`. Run all tests with:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Read the matching explanation in
[`guides/PHASE12_THEORY_QUICK_GUIDE.md`](guides/PHASE12_THEORY_QUICK_GUIDE.md).

## Phase 13: Advanced web development

Phase 13 adds AdvancedWebLab, a focused minimal API covering OpenAPI
contracts, rate limiting, health checks, safe file-upload handling, NDJSON
streaming, pagination, filtering, sorting, URL-based API versioning, WebSockets,
SignalR, and a gRPC contract companion.

Run the advanced web lab:

~~~bash
dotnet run --project AdvancedWebLab/AdvancedWebLab.csproj
~~~

Useful endpoints include /openapi/v1.json, /swagger, /health,
/api/v1/courses, /api/v2/courses, /api/v1/files, /api/v1/stream,
/ws/echo, and /hubs/courses. Run all tests with:

~~~bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
~~~

Read the matching explanation in
[guides/PHASE13_THEORY_QUICK_GUIDE.md](guides/PHASE13_THEORY_QUICK_GUIDE.md).

## Phase 14: Testing

Phase 14 adds TestingLab, a small enrollment service designed for unit testing
with deterministic dependencies, xUnit fixtures, theories, assertions, and
hand-written fakes. The phase guide also explains Moq, NSubstitute,
WebApplicationFactory, HTTP integration tests, SQLite database tests,
authentication tests, Swagger, Postman, and REST Client workflows.

Run the test project:

~~~bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
~~~

Read the matching explanation in
[guides/PHASE14_THEORY_QUICK_GUIDE.md](guides/PHASE14_THEORY_QUICK_GUIDE.md).

## Phase 15: Production and DevOps

Phase 15 adds ProductionLab with production-oriented configuration validation,
secret-safe diagnostics, JSON logging, correlation IDs, health checks,
publishing, Docker, Compose, Linux systemd, IIS, Nginx, database migration
examples, HTTPS guidance, and a GitHub Actions workflow.

Read the matching explanation in
[guides/PHASE15_THEORY_QUICK_GUIDE.md](guides/PHASE15_THEORY_QUICK_GUIDE.md).

Build the production lab with the available local SDK fallback:

~~~bash
/home/jarir-ahmed/.dotnet/dotnet build ProductionLab/ProductionLab.csproj \
  -p:TargetFramework=net9.0 --no-restore
~~~

## Phase 16: Architecture

Phase 16 adds ArchitectureLab, a commented course API demonstrating Clean
Architecture dependency direction, layered boundaries, vertical slices, CQRS,
a small mediator, repositories, unit of work, specifications, and
domain-driven design.

Read the matching explanation in
[guides/PHASE16_THEORY_QUICK_GUIDE.md](guides/PHASE16_THEORY_QUICK_GUIDE.md).

Run the architecture lab:

~~~bash
dotnet run --project ArchitectureLab/ArchitectureLab.csproj
~~~

## Project structure

```text
plan.md
guides/
  PHASE1_THEORY_QUICK_GUIDE.md
  PHASE2_THEORY_QUICK_GUIDE.md
CSharpCore/
  CSharpCore.csproj
  Program.cs
  BasicsAndControlFlow.cs
  DataStructuresAndOop.cs
  ModernCollectionsAndLinq.cs
  DelegatesLambdasAndEvents.cs
  ExceptionsAndNullability.cs
  AsyncExamples.cs
  DotNetFundamentals.cs
  DependencyInjectionExamples.cs
AspNetCoreFundamentals/
  AspNetCoreFundamentals.csproj
  Program.cs
  appsettings.json
  Middleware/
    RequestIdMiddleware.cs
    RequestLoggingMiddleware.cs
AspNetCoreWebApi/
  AspNetCoreWebApi.csproj
  Program.cs
  Controllers/ProductsController.cs
  Dtos/ProductDtos.cs
  Models/Product.cs
  Services/ProductCatalog.cs
  Infrastructure/ApiExceptionMiddleware.cs
EntityFrameworkCoreLab/
  EntityFrameworkCoreLab.csproj
  Program.cs
  Models/LearningEntities.cs
  Data/LearningDbContext.cs
  Data/LearningDbContextFactory.cs
  Queries/CourseQueryService.cs
  Migrations/README.md
SecureAspNetCoreApi/
  SecureAspNetCoreApi.csproj
  appsettings.json
  Program.cs
  Controllers/AuthController.cs
  Controllers/SecureController.cs
  Models/IdentityModels.cs
  Models/AuthDtos.cs
  Security/JwtTokenService.cs
  Security/AuthSeeder.cs
RazorPagesLab/
  RazorPagesLab.csproj
  Program.cs
  Properties/launchSettings.json
  Models/Workshop.cs
  Models/EnrollmentInput.cs
  Services/WorkshopCatalog.cs
  Pages/
    Index.cshtml
    Index.cshtml.cs
    Enroll.cshtml
    Enroll.cshtml.cs
    EnrollmentComplete.cshtml
    Shared/_Layout.cshtml
    Shared/_WorkshopCard.cshtml
  wwwroot/css/site.css
MvcLab/
  MvcLab.csproj
  Program.cs
  Properties/launchSettings.json
  Models/Book.cs
  ViewModels/
    BookCardViewModel.cs
    BookListViewModel.cs
    BookDetailsViewModel.cs
    BookCreateViewModel.cs
  Services/BookCatalog.cs
  Infrastructure/Filters/RequestAuditFilter.cs
  Controllers/BooksController.cs
  Controllers/HomeController.cs
  Views/
    Books/Index.cshtml
    Books/Details.cshtml
    Books/Create.cshtml
    Shared/_Layout.cshtml
    Shared/_BookCard.cshtml
  wwwroot/css/site.css
BlazorLab/
  BlazorLab.csproj
  Program.cs
  Properties/launchSettings.json
  Models/CourseSummary.cs
  Models/EnrollmentInput.cs
  Models/BrowserSnapshot.cs
  Services/LearningState.cs
  Services/BrowserClockReceiver.cs
  Components/App.razor
  Components/Routes.razor
  Components/_Imports.razor
  Components/Layout/MainLayout.razor
  Components/Layout/NavMenu.razor
  Components/Shared/CourseCard.razor
  Components/Pages/Home.razor
  Components/Pages/Enrollment.razor
  Components/Pages/Lifecycle.razor
  Components/Pages/JavaScriptInterop.razor
  wwwroot/app.css
  wwwroot/js/interop.js
AdvancedAspNetCoreLab/
  AdvancedAspNetCoreLab.csproj
  Program.cs
  appsettings.json
  Properties/launchSettings.json
  Configuration/CoursePlatformOptions.cs
  Models/CatalogModels.cs
  Models/JobModels.cs
  DI/ClockServices.cs
  DI/MessageEnvelope.cs
  Caching/CatalogCacheService.cs
  Background/BackgroundTaskQueue.cs
  Background/JobStore.cs
  Background/QueuedBackgroundService.cs
  Background/CatalogRefreshHandler.cs
AdvancedWebLab/
  AdvancedWebLab.csproj
  Program.cs
  appsettings.json
  Models/CourseModels.cs
  Services/CourseCatalog.cs
  Health/CourseCatalogHealthCheck.cs
  Hubs/CourseHub.cs
  Protos/course.proto
  Grpc/README.md
TestingLab/
  TestingLab.csproj
  EnrollmentModels.cs
  EnrollmentPorts.cs
  EnrollmentExceptions.cs
  EnrollmentService.cs
  DefaultImplementations.cs
  ApiRequests.http
  Postman/phase14-advanced-web.postman_collection.json
ProductionLab/
  ProductionLab.csproj
  Program.cs
  appsettings.json
  appsettings.Production.json
  Dockerfile
  Configuration/ProductionOptions.cs
  Middleware/CorrelationIdMiddleware.cs
  Middleware/RequestAuditMiddleware.cs
  Health/ProductionReadinessCheck.cs
  Services/AuditStore.cs
  Database/
  Hosting/
ArchitectureLab/
  ArchitectureLab.csproj
  Program.cs
  Domain/
  Application/
  Infrastructure/
  Api/Features/Courses/
CSharpCore.Tests/
  CSharpCore.Tests.csproj
  BasicsAndControlFlowTests.cs
  DataStructuresAndOopTests.cs
  ModernCollectionsAndLinqTests.cs
  DelegatesLambdasAndEventsTests.cs
  ExceptionsAndNullabilityTests.cs
  AsyncExamplesTests.cs
  DotNetFundamentalsTests.cs
  DependencyInjectionTests.cs
  AspNetCoreFundamentalsTests.cs
  AspNetCoreWebApiTests.cs
  EntityFrameworkCoreTests.cs
  SecureAspNetCoreApiTests.cs
  RazorPagesTests.cs
  MvcTests.cs
  BlazorTests.cs
  AdvancedAspNetCoreTests.cs
  AdvancedWebTests.cs
  TestingLabUnitTests.cs
  ProductionLabTests.cs
  ArchitectureLabTests.cs
```

The course will add ASP.NET Core projects only after the language and .NET
fundamentals are established.
# CSharp_ASP_RAZOR_BLAZOR
