# Phase 10: Blazor fundamentals

Phase 10 introduces Blazor through `BlazorLab`, a modern Blazor Web App using
Interactive Server rendering. Blazor components are reusable C# and Razor
units that render HTML and respond to UI events.

## 1. What Blazor adds

Razor Pages and MVC render a page for a request. Blazor adds a component state
and event model on top of Razor syntax:

```text
Browser event
  -> component event handler
  -> component state changes
  -> Blazor renders the changed component
```

The component code executes on the server in this phase. The browser connects
through the Blazor circuit and sends events to the server. The server sends the
resulting UI updates back to the browser.

## 2. The application entry point

`Program.cs` registers Razor components and Interactive Server support:

```csharp
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
```

The endpoint maps the root `App` component and enables the server render mode:

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

`Components/App.razor` supplies the HTML document, `HeadOutlet`, routed content,
and the `blazor.web.js` client script. `Components/Routes.razor` connects URLs
to components with `@page` directives.

## 3. Components and routing

The home component declares its route like this:

```razor
@page "/"
```

The enrollment and lifecycle components use `/enroll` and `/lifecycle`. The
shared `MainLayout` wraps routed pages and `NavMenu` uses `NavLink` to show the
active route.

A component can contain three kinds of content:

- markup that describes the rendered HTML;
- event attributes such as `@onclick`;
- an `@code` block containing fields, properties, methods, and lifecycle hooks.

## 4. Parameters and EventCallback

`CourseCard.razor` receives a course from its parent:

```razor
<CourseCard Course="course" Selected="SelectCourse" />
```

The child declares the matching parameters:

```csharp
[Parameter, EditorRequired]
public CourseSummary Course { get; set; } = default!;

[Parameter]
public EventCallback<CourseSummary> Selected { get; set; }
```

The child invokes the callback when its button is clicked. This keeps the child
reusable: it reports what happened while the parent decides what selected
course state means.

## 5. Events and state

Blazor event handlers are ordinary C# methods connected to markup:

```razor
<button @onclick="IncrementClickCount">Increase count</button>
```

The `LearningState` service is registered as scoped. In Interactive Server
rendering, this gives each connected circuit its own state instance. The home
component subscribes to the service's `Changed` event and calls
`InvokeAsync(StateHasChanged)` when state changes outside the immediate event
handler.

Always unsubscribe from service events in `Dispose`. Otherwise an old component
can remain referenced after navigation and continue receiving notifications.

## 6. Forms, binding, and validation

`Enrollment.razor` uses `EditForm` with a DataAnnotations model:

```razor
<EditForm Model="Input" OnValidSubmit="Submit" FormName="course-enrollment">
    <DataAnnotationsValidator />
    <ValidationSummary />
    <InputText @bind-Value="Input.FullName" />
    <ValidationMessage For="() => Input.FullName" />
</EditForm>
```

The input components bind browser values to C# properties:

- `InputText` binds text values;
- `InputNumber` parses numeric values;
- `InputSelect` binds a selected option;
- `ValidationMessage` displays one field's errors;
- `ValidationSummary` displays the form's errors.

`OnValidSubmit` runs only after validation passes. Use `OnInvalidSubmit` when a
component needs a separate response to invalid input. Server-side validation
still matters because clients can send requests without using the rendered UI.

## 7. Dependency injection and state lifetime

The component injects the state service with:

```razor
@inject LearningState LearningState
```

The service is registered with `AddScoped`. Lifetime choice matters:

```text
Singleton -> shared by the whole application
Scoped    -> shared within one web request or Blazor circuit
Transient -> a new instance each time it is requested
```

Do not put one user's private interactive state in a singleton. Use a scoped
service for circuit state and a database or durable service for data that must
survive reconnects and application restarts.

## 8. Component lifecycle

`Lifecycle.razor` demonstrates:

```text
OnInitialized
OnParametersSet
OnAfterRenderAsync
```

Initialization is a good place to prepare local state. Parameter processing is
where a component reacts to changed parameters. `OnAfterRenderAsync` runs after
rendering and is the appropriate place for work that needs rendered DOM or
browser interop. Calling `StateHasChanged` there without a first-render guard
can create a rendering loop.

Use asynchronous lifecycle methods for I/O and do not block the circuit with
long synchronous work.

## 9. Rendering models

This phase uses Interactive Server, but modern Blazor supports several places
where components can execute:

- static server rendering produces HTML without ongoing interactivity;
- Interactive Server keeps component code on the server;
- Interactive WebAssembly runs component code in the browser;
- automatic approaches can choose an interactive location based on the app's
  configuration.

The key design question is where component code runs, where state lives, and
what data must cross the network. Interactive Server simplifies access to
server services but depends on a live circuit. WebAssembly can reduce server
interaction after download but requires client-compatible code and assets.

## 10. What this lesson leaves for later

The sample keeps state in memory and does not persist enrollment to a database.
It also does not cover JavaScript interop, browser storage, authentication,
authorization, or production circuit scaling. Those concerns should be added
with an explicit state and deployment design rather than hidden in components.

## Run the phase

From the repository root:

```bash
dotnet run --project BlazorLab/BlazorLab.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Open `/` for the interactive course cards, `/enroll` for the validated form,
and `/lifecycle` for the lifecycle example.
