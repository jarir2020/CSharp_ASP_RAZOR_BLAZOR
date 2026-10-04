# Phase 3: .NET Fundamentals

Phase 3 explains the platform underneath ASP.NET Core. The code demonstrates
runtime information, assemblies, the Base Class Library, Garbage Collection,
the .NET CLI, project artifacts, and dependency injection.

## 1. C# and .NET have different roles

- **C#** is the programming language.
- **.NET** is the runtime, libraries, and development platform.
- **The .NET SDK** contains the CLI, compilers, templates, and build tools.
- **The .NET runtime** executes an already-built application.
- **ASP.NET Core** is the web framework built on .NET.

The same C# source can be compiled into an assembly and run by the .NET
runtime. A web application adds ASP.NET Core on top of that platform.

## 2. CLR, JIT, and AOT

The Common Language Runtime (CLR) provides services such as type safety,
exception handling, garbage collection, and thread management. The runtime
loads the compiled Intermediate Language (IL) from an assembly.

With just-in-time (JIT) compilation, the runtime compiles methods to native
machine code as they are needed. Native AOT compiles more of the application
ahead of time. JIT is the normal starting point for learning ASP.NET Core;
AOT is a deployment choice with different startup, size, and reflection
tradeoffs.

## 3. Assemblies and DLLs

An assembly is a compiled unit containing IL, metadata, and referenced
information. A class library commonly produces a `.dll`; an executable project
also has an application entry point and can produce an executable host.

```csharp
Assembly assembly = typeof(DotNetFundamentals).Assembly;
string name = assembly.GetName().Name ?? "Unknown";
```

The project uses this reflection example to inspect the assembly that contains
the lesson code. Reflection is useful for infrastructure and frameworks, but
application code should avoid depending on hidden type discovery when a direct
dependency is clearer.

## 4. The Base Class Library

The Base Class Library (BCL) provides common building blocks such as:

```text
System.String
System.Collections.Generic.List<T>
System.IO.File
System.Threading.Tasks.Task
System.Net.Http.HttpClient
```

These types are available because they ship with .NET. A NuGet package is an
additional library distributed outside the base runtime. The Phase 3 project
uses the `Microsoft.Extensions.DependencyInjection` NuGet package for the
same dependency injection container used across ASP.NET Core applications.

## 5. Garbage Collection

Managed objects are allocated on the .NET managed heap. The Garbage Collector
(GC) finds objects that are no longer reachable and reclaims their memory.
This means application code normally does not call `free` for ordinary managed
objects.

The GC does not replace cleanup for external resources. Files, sockets,
database connections, and similar resources should still be disposed through
`using`, `await using`, or an owning framework abstraction.

`GC.GetTotalMemory` is useful for a small lesson, but it is not a production
memory-monitoring strategy. Use runtime metrics and observability tools for
real applications.

## 6. The .NET CLI

The SDK exposes the normal project workflow through `dotnet` commands:

```bash
dotnet --info
dotnet new console
dotnet restore
dotnet build
dotnet run
dotnet test
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet publish
```

`restore` downloads declared NuGet dependencies. `build` compiles the project.
`run` builds and starts it. `test` runs test projects. `publish` creates a
deployment output and should be validated with the target hosting environment.

## 7. Project structure

The main files in a small .NET project are:

- `.csproj`: target framework, compiler settings, project references, and
  package references.
- `Program.cs`: application entry point. Modern templates often use top-level
  statements here.
- `bin/`: generated build and runtime output.
- `obj/`: generated intermediate files used by MSBuild.
- `appsettings.json`: a common configuration file used by ASP.NET Core
  projects. This console lesson does not need it yet.

`bin/` and `obj/` are generated artifacts. They should normally be ignored by
source control and can be recreated from the source project.

## 8. Dependency injection

Dependency injection (DI) means an object receives the collaborators it needs
instead of constructing them internally.

```csharp
public GreetingService(IClock clock, IGreetingFormatter formatter)
{
    _clock = clock;
    _formatter = formatter;
}
```

The class depends on interfaces. A composition root registers the concrete
implementations:

```csharp
services.AddSingleton<IClock, SystemClock>();
services.AddTransient<IGreetingFormatter, GreetingFormatter>();
services.AddScoped<GreetingService>();
```

The three common lifetimes are:

- **Transient** creates a new instance each time it is requested.
- **Scoped** creates one instance per scope. In ASP.NET Core, a scope usually
  represents one HTTP request.
- **Singleton** creates one instance for the service provider lifetime.

The correct lifetime depends on state and thread safety. A singleton should be
safe for concurrent use and should not capture a scoped service. A scoped
service should not be stored in a singleton. `ValidateScopes` and
`ValidateOnBuild` help catch registration mistakes early.

## 9. Why DI matters for testing

`GreetingService` receives an `IClock`, so a test can pass `FixedClock` instead
of waiting for the real system clock. The business behavior becomes
deterministic and the service does not need special test-only branches.

The same idea will be used later for database contexts, repositories, HTTP
clients, authentication services, and application configuration.

## Run the phase

From the repository root:

```bash
dotnet run --project CSharpCore/CSharpCore.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

The next phase begins ASP.NET Core fundamentals: HTTP, `Program.cs`, the
middleware pipeline, configuration, logging, routing, and endpoints.
