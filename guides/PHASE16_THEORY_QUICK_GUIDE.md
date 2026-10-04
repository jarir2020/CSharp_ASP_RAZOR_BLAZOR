# Phase 16: Architecture

Phase 16 adds ArchitectureLab, a small course API that makes several
architectural choices visible without hiding them behind a large framework.
The sample includes a domain aggregate, application request handlers,
repository and unit-of-work ports, a specification, an in-memory
infrastructure adapter, and vertical-slice endpoints.

## 1. Clean Architecture dependency direction

The important rule is the direction of source dependencies:

~~~text
Domain
  <- Application
       <- Infrastructure
       <- API
~~~

The domain contains business rules and does not know about HTTP, JSON, a
database, or a logging provider. Application code coordinates use cases through
interfaces. Infrastructure implements those interfaces. The API is the
composition root that connects implementations to use cases.

ArchitectureLab keeps these areas visible:

~~~text
Domain/
Application/
Infrastructure/
Api/Features/
~~~

This separation is useful when business rules must be tested without starting
an HTTP server or opening a database. It has a cost: more files and interfaces
must be justified by a real boundary.

## 2. Layered architecture

A traditional layered application commonly looks like:

~~~text
API / Controllers
Application / Services
Domain / Models
Infrastructure / Data access
~~~

Requests move down through layers and results move back up. Layering is easy to
explain and works well when the application has a stable set of broad
responsibilities. Keep the rules for each layer clear. If every layer simply
forwards the same method call, the project may have ceremony without useful
isolation.

## 3. Vertical slice architecture

Vertical slices organize code by a user-facing feature instead of by technical
type. ArchitectureLab maps the course feature through:

~~~text
Api/Features/Courses/CreateCourseSlice.cs
Api/Features/Courses/GetCourseSlice.cs
Api/Features/Courses/ListCoursesSlice.cs
~~~

Each slice owns the route-specific request flow while it reuses the application
ports and domain rules. A larger application might put the command, validator,
handler, endpoint, and response model in one feature folder. This reduces the
number of places a developer must visit when changing one use case.

Layered and vertical-slice organization can coexist. Use layers for stable
technical boundaries and slices for feature-specific behavior.

## 4. Domain-Driven Design

DDD starts with the business language and boundaries:

~~~text
Course
CourseId
course level
course price
course creation rules
~~~

Course is an aggregate root. Its factory enforces title, level, ID, and price
rules. The API cannot create an invalid Course and then ask another class to
repair it later.

CourseId is a value object. It gives the course identity a domain name and
prevents unrelated IDs from being passed accidentally. In a larger domain,
other value objects might represent EmailAddress, Money, or TenantId.

DDD does not mean every table becomes a class with dozens of methods. Model
business behavior where the business rules are meaningful. Simple read-only
data may remain a record or projection.

## 5. CQRS

CQRS separates commands that change state from queries that read state:

~~~text
CreateCourseCommand -> CreateCourseHandler -> repository + unit of work
GetCourseQuery       -> GetCourseHandler
ListCoursesQuery     -> ListCoursesHandler + specification
~~~

The sample uses separate request and response types. Commands return the
created response here for an HTTP location; another system might return only an
ID. Queries do not call the unit of work because they do not change state.

CQRS is a boundary, not a requirement to use two databases. Start with
separate models when read and write behavior are genuinely different. Do not
split a tiny CRUD form into a large command hierarchy without a reason.

## 6. Mediator and request handlers

RequestDispatcher is a small educational mediator. It finds the handler
registered for a request type and calls it:

~~~csharp
CourseResponse? course = await dispatcher.SendAsync(
    new GetCourseQuery(courseId),
    cancellationToken);
~~~

The mediator keeps endpoints from knowing which handler class implements a use
case. A real application may use MediatR and add pipeline behaviors for
validation, logging, authorization, transactions, or timing.

The project intentionally implements the small mechanism directly so the
dependency-injection relationship stays visible. If MediatR is introduced,
register its package in the application project and keep the handlers focused
on one request. A package does not remove the need to decide where business
rules belong.

## 7. Repository pattern

ICourseRepository is an application port:

~~~csharp
Task<Course?> GetByIdAsync(
    CourseId id,
    CancellationToken cancellationToken);
~~~

InMemoryCourseRepository is the infrastructure adapter. An EF Core adapter
could implement the same port with DbContext and LINQ. The application handler
does not need to know whether data came from memory, SQLite, or a remote
service.

Repositories are useful when they protect a meaningful aggregate boundary or
hide a data source that may change. A generic repository with methods for every
possible query can obscure EF Core instead of helping. Prefer a focused port
that matches a use case.

## 8. Unit of Work

IUnitOfWork represents the commit boundary:

~~~csharp
await repository.AddAsync(course, cancellationToken);
await unitOfWork.CommitAsync(cancellationToken);
~~~

The in-memory implementation increments a counter. A database implementation
could commit a transaction containing several aggregate changes. If the data
access technology already provides a reliable unit-of-work boundary, adding a
second wrapper may be unnecessary. The abstraction should express a real
transaction or consistency decision.

## 9. Specification pattern

CourseLevelSpecification represents a reusable business query rule:

~~~csharp
CourseLevelSpecification specification = new("advanced");
IReadOnlyList<Course> courses = await repository.ListAsync(
    specification,
    cancellationToken);
~~~

The specification keeps the filtering idea named and testable. An EF Core
implementation might represent the same rule as an expression tree so the
database can execute it. Do not create a specification class for a one-line
filter that has no reuse or domain meaning.

## 10. API composition root

Program.cs registers concrete infrastructure and maps the slices:

~~~csharp
builder.Services.AddArchitectureApplication();

RouteGroupBuilder courses = app.MapGroup("/api/courses");
courses.MapCreateCourseSlice();
courses.MapGetCourseSlice();
courses.MapListCoursesSlice();
~~~

This is the composition root. It is the place where the application chooses
the repository and unit-of-work implementation. Keeping this decision near
startup makes tests and deployment replacements easier.

## 11. Testing architecture

ArchitectureLabTests exercises the domain directly and builds the application
services in memory. It verifies:

~~~text
aggregate invariants
value-object behavior
specification filtering
dispatcher-to-handler routing
repository and query behavior
~~~

These tests do not need a web server or database. The earlier phases already
contain HTTP, SQLite, authentication, and production-host integration tests.
Together they show why architecture should preserve testable boundaries rather
than merely create more folders.

## Run the phase

~~~bash
dotnet run --project ArchitectureLab/ArchitectureLab.csproj
curl http://localhost:5491/api/courses
curl http://localhost:5491/api/courses/11111111-1111-1111-1111-111111111111
curl 'http://localhost:5491/api/courses?level=Advanced'
curl -X POST http://localhost:5491/api/courses \
  -H 'Content-Type: application/json' \
  -d '{"title":"Architecture Fundamentals","level":"Advanced","price":175}'
~~~

Run the repository tests with:

~~~bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
~~~

The full solution should stay understandable. Add an abstraction when it
protects a business rule, a dependency boundary, a transaction, or a test
seam. Remove it when it only repeats the next method call.
