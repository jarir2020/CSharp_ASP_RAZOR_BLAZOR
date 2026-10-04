# Phase 1: C# Fundamentals

This phase builds the language foundation needed before ASP.NET Core. Each
example is intentionally small. The main application in `CSharpCore/` prints
the examples, while `CSharpCore.Tests/` turns the expected behavior into
repeatable checks.

## 1. Variables and types

A variable has a declared type that tells the compiler which values and
operations are valid. Common starting types include `int`, `decimal`, `bool`,
`char`, and `string`.

```csharp
int userId = 42;
decimal price = 19.99m;
bool isActive = true;
string username = "jarir";
```

The `m` suffix tells C# that `19.99` is a `decimal`. `decimal` is commonly
used for money because it represents base-10 values more predictably than a
binary floating-point type.

## 2. Operators and conditions

Arithmetic operators calculate values. Comparison operators produce a
`bool`, which can control an `if` statement.

```csharp
int remainder = 10 % 3; // 1

if (remainder == 1)
{
    Console.WriteLine("The remainder is one.");
}
```

The conditional operator is a compact expression form of `if/else`:

```csharp
string status = isActive ? "Active" : "Inactive";
```

## 3. Enums

An enum gives readable names to a fixed set of choices:

```csharp
public enum Grade
{
    A,
    B,
    C,
    F
}
```

Returning `Grade.A` communicates more clearly than returning an unexplained
number such as `0`.

## 4. Loops and arrays

Use a `for` loop when the counter and boundaries are known. Use a `while`
loop when repetition depends on a condition that changes during the loop.

```csharp
for (int number = 1; number <= 5; number++)
{
    Console.WriteLine(number);
}

int[] numbers = { 10, 20, 30, 40, 50 };
int[] middle = numbers[1..4]; // 20, 30, 40
```

The range `1..4` starts at index 1 and stops before index 4.

## 5. Collections

- `List<T>` stores an ordered sequence that can grow.
- `Dictionary<TKey, TValue>` looks up a value by a key.
- `HashSet<T>` stores unique values.

The `<T>` notation means the collection is generic: the caller chooses the
element type while the compiler still checks it.

## 6. Classes, properties, and constructors

A class combines state and behavior. A constructor prepares a valid object,
and properties provide controlled access to its state.

```csharp
public sealed class User
{
    public User(string username)
    {
        Username = username;
        IsActive = true;
    }

    public string Username { get; }
    public bool IsActive { get; private set; }
}
```

`private set` lets the class change `IsActive` while preventing unrelated code
from assigning an invalid state directly. This is encapsulation.

## 7. Structs and classes

A small `struct` is a value type. Assigning it copies its value. A `class` is
a reference type. Assigning it copies a reference to the same object. The
course uses an immutable `Coordinates` struct and a mutable `SimpleUser`
class to make the distinction visible.

## 8. Interfaces, inheritance, and composition

An interface describes a capability:

```csharp
public interface IMessageSender
{
    string Send(string message);
}
```

An abstract base class can share common behavior and require derived classes
to implement a missing part. A derived class can replace virtual behavior with
`override`.

Composition means an object receives another object that it needs. In the
lesson, `PremiumAccount` receives an `IMessageSender`, so the account depends
on the capability rather than a specific message-sender implementation.

## 9. `params` and generics

`params decimal[] amounts` lets a method accept a variable number of decimal
arguments. A generic method such as `FirstItem<T>` works for many types while
remaining type-safe.

## Run the phase

From the repository root:

```bash
dotnet run --project CSharpCore/CSharpCore.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

The test project uses xUnit. The next phase will introduce modern C# features,
nullable reference types, exceptions, LINQ, and asynchronous programming.
