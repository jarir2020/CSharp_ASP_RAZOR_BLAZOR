# Phase 14: Testing

Phase 14 treats tests as executable design examples. The new TestingLab
contains a small enrollment service with explicit dependencies. The service is
small enough to understand, but it still has validation, a pricing rule,
duplicate protection, persistence, notification, time, and ID generation.

## 1. The test pyramid

Use the cheapest reliable test level that can prove the behavior:

~~~text
unit tests
  one class, deterministic collaborators, no network or database

integration tests
  several application components, often through a real HTTP or database boundary

system or API tests
  a running application and a client-oriented contract
~~~

Unit tests should explain business rules quickly. Integration tests should
prove that registration, routing, serialization, dependency injection,
authentication, and persistence are wired together. API tools help a developer
explore the public contract manually and save repeatable request collections.

## 2. xUnit assertions and test data

This repository uses xUnit. A basic test has a setup, an action, and an
assertion:

~~~csharp
EnrollmentReceipt receipt = await service.EnrollAsync(request);

Assert.Equal("learner@example.com", receipt.StudentEmail);
Assert.Equal(112.50m, receipt.Price);
~~~

Use Theory and InlineData for a small set of equivalent inputs:

~~~csharp
[Theory]
[InlineData("")]
[InlineData("missing-at-symbol")]
public async Task Invalid_email_is_rejected(string email)
{
    await Assert.ThrowsAsync<ArgumentException>(() => service.EnrollAsync(
        new EnrollmentRequest(email, 7, EarlyBird: false)));
}
~~~

The EnrollmentScenarioFixture demonstrates IClassFixture. The fixture stores
stable setup and creates fresh mutable doubles for each test. Shared fixtures
must not allow one test to change the result of another test.

NUnit offers similar ideas with Test, TestCase, SetUp, and assertions. Choose
the framework that matches the repository and team tooling; the important
decisions are readable behavior, isolated state, and useful failure messages.

## 3. Unit testing through dependency seams

EnrollmentService does not create a database connection, read the system
clock, generate a random ID, or send an email directly. It receives interfaces:

~~~text
ICourseCatalog
IEnrollmentStore
IEnrollmentNotifier
ISystemClock
IEnrollmentIdGenerator
~~~

The unit tests provide fake implementations. This makes the success path
repeatable and lets failure tests verify that later side effects did not occur.
For example, the duplicate test checks both the exception and the fact that no
record or confirmation was written.

Hand-written fakes are often the clearest choice when the behavior matters and
the fake is short. Their named state is visible in a debugger. They can become
too large when a dependency has many members, which is where a mocking library
can help.

## 4. Moq

Moq creates a test double and lets a test configure return values and verify
calls. The following is an equivalent setup for the catalog and notifier:

~~~csharp
var catalog = new Mock<ICourseCatalog>();
catalog
    .Setup(item => item.FindAsync(7, It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Course(7, "Testing ASP.NET Core", 125m));

var notifier = new Mock<IEnrollmentNotifier>();
await service.EnrollAsync(
    new EnrollmentRequest("learner@example.com", 7, EarlyBird: false));

notifier.Verify(
    item => item.SendConfirmationAsync(
        It.IsAny<EnrollmentReceipt>(),
        It.IsAny<CancellationToken>()),
    Times.Once);
~~~

Use verification for an interaction that is part of the behavior, such as
publishing a required confirmation. Avoid verifying every internal call; that
couples a test to implementation details.

## 5. NSubstitute

NSubstitute uses a concise substitute style:

~~~csharp
ICourseCatalog catalog = Substitute.For<ICourseCatalog>();
catalog
    .FindAsync(7, Arg.Any<CancellationToken>())
    .Returns(new Course(7, "Testing ASP.NET Core", 125m));

IEnrollmentNotifier notifier = Substitute.For<IEnrollmentNotifier>();
await service.EnrollAsync(
    new EnrollmentRequest("learner@example.com", 7, EarlyBird: false));

await notifier.Received(1).SendConfirmationAsync(
    Arg.Any<EnrollmentReceipt>(),
    Arg.Any<CancellationToken>());
~~~

Moq and NSubstitute are alternatives. A test project usually standardizes on
one mocking style so setup and verification remain familiar.

## 6. Integration testing with WebApplicationFactory

WebApplicationFactory starts an ASP.NET Core application inside a test host and
returns an HttpClient. Existing tests demonstrate this at several boundaries:

~~~text
AspNetCoreWebApiTests       -> model binding, validation, HTTP status codes
AdvancedAspNetCoreTests     -> caching, DI, and background work
AdvancedWebTests            -> OpenAPI, health, upload, rate limit, SignalR
MvcTests and RazorPagesTests -> HTML routes, forms, antiforgery, redirects
~~~

The test host can use a Testing environment and replace services before the
application starts. This is how SecureAspNetCoreApiTests replace the production
database with an in-memory SQLite connection.

## 7. Database and authentication integration tests

EntityFrameworkCoreTests uses a real SQLite connection with an in-memory
database. It verifies schema creation, seed data, relationships, queries,
tracking behavior, transactions, and a duplicate enrollment rule.

SecureAspNetCoreApiTests exercises the HTTP boundary around registration,
password hashing, JWT login, protected endpoints, roles, permission claims,
and CORS. It also uses a test database so tests do not depend on a developer's
local database file.

These tests are more expensive than pure unit tests, but they cover wiring that
fakes cannot prove.

## 8. API testing tools

The Phase 13 application exposes a contract at /openapi/v1.json and a small
browser page at /swagger. Use those endpoints to inspect the request and
response surface while developing.

The repository also includes:

~~~text
TestingLab/ApiRequests.http
TestingLab/Postman/phase14-advanced-web.postman_collection.json
~~~

The HTTP file works with REST Client extensions in editors that support the
format. Import the JSON file into Postman, set the baseUrl variable, and run
the requests individually. For rate limiting, send the request three times in
the same fixed window and expect the third response to be 429.

A successful manual request is useful evidence, but it should not replace an
automated regression test for a bug or contract that must remain stable.

## 9. Test naming and scope

Name a test after the observable behavior:

~~~text
Successful_enrollment_normalizes_email_applies_discount_and_notifies
Duplicate_enrollment_throws_before_writing_or_notifying
~~~

Keep each test focused on one behavior. Assert the important response or
domain result and the side effects that form part of the contract. Avoid
asserting timestamps, generated IDs, log wording, or private helper calls
unless those values are explicitly part of the requirement.

## Run the phase

~~~bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
~~~

To inspect the API examples, start the Phase 13 application first:

~~~bash
dotnet run --project AdvancedWebLab/AdvancedWebLab.csproj
~~~

Then open /swagger, use TestingLab/ApiRequests.http, or import the Postman
collection. The new unit tests are in
CSharpCore.Tests/TestingLabUnitTests.cs.
