# Phase 2: Modern C#

Phase 2 introduces the C# features used constantly in .NET applications:
collections, LINQ, delegates, lambdas, events, exceptions, nullable values,
and asynchronous programming. The examples remain local and deterministic so
the tests focus on language behavior.

## 1. Collection types

Use a collection according to the operation the code needs:

- `List<T>` stores an ordered, growable sequence.
- `Dictionary<TKey, TValue>` looks up a value by key.
- `HashSet<T>` stores unique values.
- `Queue<T>` processes items first-in, first-out.
- `Stack<T>` processes items last-in, first-out.

```csharp
Queue<string> requests = new();
requests.Enqueue("first");
string next = requests.Dequeue(); // first

Stack<string> pages = new();
pages.Push("home");
string previous = pages.Pop(); // home
```

The project also demonstrates collection interfaces:

- `IEnumerable<T>` means the values can be enumerated.
- `ICollection<T>` adds operations such as `Add` and `Count`.
- `IList<T>` adds index-based access.

Accepting an interface instead of a concrete `List<T>` makes a method easier
to reuse and test.

## 2. LINQ

LINQ (Language Integrated Query) applies query operations to an enumerable
sequence. The operations are composable, so each method can describe one part
of the transformation.

```csharp
var activeNames = users
    .Where(user => user.IsActive)
    .OrderBy(user => user.Name)
    .Select(user => user.Name)
    .ToList();
```

Important operators in this phase:

- `Where` filters.
- `Select` projects each item into another value.
- `SelectMany` flattens nested collections.
- `First` and `Single` express required results and throw when their rules are
  violated.
- `FirstOrDefault` and `SingleOrDefault` allow a missing result to become
  `null` for reference types.
- `Any`, `All`, `Count`, and `Sum` answer common existence, validation, and
  aggregation questions.
- `OrderBy` and `ThenBy` sort.
- `GroupBy` creates groups.
- `Join` matches related sequences through keys.
- `Distinct` removes duplicates and `Contains` checks membership.

LINQ queries are often deferred: the query describes work that happens when
the sequence is enumerated. `ToList()` materializes the result immediately,
which is useful when the method should return a stable snapshot.

## 3. Delegates and lambdas

A delegate is a type-safe reference to a method. A lambda is a short function
expression that can be assigned to a delegate or passed directly.

```csharp
PriceRule discount = price => price * 0.90m;
decimal result = discount(100m);
```

The common generic delegates are:

- `Action<T>` receives a value and returns nothing.
- `Func<T, TResult>` receives a value and returns a result.
- `Predicate<T>` receives a value and returns `bool`.

This allows a method to receive behavior as an argument. For example, the
same filtering method can accept a rule for names that start with `A`, names
that are longer than four characters, or any other predicate.

## 4. Events

An event lets an object announce that something happened without knowing which
objects are listening.

```csharp
reporter.ProgressChanged += (_, args) =>
    Console.WriteLine(args.Percentage);

reporter.Report(50);
```

The publisher owns the event and raises it. Subscribers attach and detach
handlers. In web applications, this pattern appears in UI notifications,
progress reporting, and application lifecycle integrations.

## 5. Exceptions and custom errors

Use exceptions for exceptional control flow and validate inputs at clear
boundaries. A custom exception can carry a domain-specific error code:

```csharp
public sealed class CourseValidationException : Exception
{
    public CourseValidationException(string code, string message)
        : base(message) => Code = code;

    public string Code { get; }
}
```

`try` contains risky work, `catch` handles a known failure, and `finally`
runs cleanup code on both success and failure.

```csharp
try
{
    result = numerator / denominator;
}
catch (DivideByZeroException)
{
    result = 0;
}
finally
{
    cleanup();
}
```

Do not catch every exception and silently continue. At an application
boundary, catch errors you can handle, add useful context, or let a global
handler translate them into an appropriate API response.

## 6. Nullable reference and value types

With nullable reference types enabled, `string` means a string should exist and
`string?` means `null` is allowed. Value types use the same idea with `int?`
and `DateTime?`.

```csharp
string? nickname = null;
string displayName = nickname ?? "Guest";
int? age = null;
string shownAge = age?.ToString() ?? "Unknown";
```

The compiler uses these annotations to warn about possible null dereferences.
The annotations do not automatically validate data received from a request,
database, or user. Runtime validation is still required.

## 7. Async, `Task`, and cancellation

`Task` represents work that may finish later. `Task<T>` represents future work
that produces a `T`. `await` pauses the current method until the task finishes
without blocking the thread synchronously.

```csharp
public async Task<string> LoadAsync(CancellationToken cancellationToken)
{
    await Task.Delay(5, cancellationToken);
    return "Loaded";
}
```

`CancellationToken` gives the caller a way to stop work that is no longer
needed, such as a request that disconnected. Methods should pass the token to
the operations they call and check it during longer loops.

When independent I/O operations can run together, start their tasks and await
`Task.WhenAll`. `WhenAll` preserves the order of its task collection in its
returned results.

## 8. Async I/O versus CPU parallelism

Async I/O waits efficiently for external work such as a database or HTTP
response. `Parallel.For` is for CPU-bound work that can be divided safely
across threads. They solve different problems. Do not wrap ordinary database
or HTTP calls in `Task.Run`; use the API's native asynchronous method instead.

The phase demo uses `Task.Delay` to simulate I/O and `Parallel.For` for a small
CPU-bound doubling operation so the distinction is visible without external
services.

## Run the phase

From the repository root:

```bash
dotnet run --project CSharpCore/CSharpCore.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

The next phase moves from language features into .NET itself: the runtime, SDK,
CLI, project files, assemblies, NuGet, and dependency injection.
