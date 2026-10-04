# Agent instructions

## Project purpose

This repository is a beginner-friendly C#, .NET, ASP.NET Core, Razor, and
Blazor learning course. The learner should understand why the code works, not
only receive a working result.

## Before changing code

- Read `plan.md` and the relevant `guides/PHASE*_THEORY_QUICK_GUIDE.md`.
- Inspect the existing implementation and tests before editing.
- Preserve unrelated user changes and do not delete project files casually.
- Keep each phase small enough to read and run independently.

## Coding style

- Add concise comments when C# or .NET behavior may be unfamiliar to a
  beginner.
- Prefer small deterministic examples over hidden framework magic.
- Keep lesson code focused on one concept at a time.
- Never put passwords, API keys, tokens, or local `.env` values in source,
  documentation, commits, or command output.

## Validation

Run the checks relevant to the change when the .NET SDK is available:

```bash
dotnet build CSharpCore/CSharpCore.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
dotnet run --project CSharpCore/CSharpCore.csproj
```

Do not describe source inspection as a successful build or test run.

## Documentation

When a phase changes, update the matching guide and `README.md`. Explain the
request path, tradeoffs, and production limitations in plain language as the
course reaches web development topics.
