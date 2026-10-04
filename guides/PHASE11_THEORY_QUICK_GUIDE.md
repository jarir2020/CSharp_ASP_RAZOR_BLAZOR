# Phase 11: JavaScript interop

Phase 11 extends `BlazorLab` with a `/interop` page. Blazor covers most UI
work in C#, but browser APIs and existing JavaScript libraries still require a
bridge between .NET and JavaScript.

## 1. The interop boundary

The browser and the server are different execution environments in Interactive
Server rendering:

```text
Blazor component on server
  <-> Blazor circuit
Browser JavaScript and DOM
```

JavaScript interop is an asynchronous boundary. A C# call serializes arguments
to JavaScript, JavaScript performs browser work, and the result is serialized
back to .NET when the call returns.

## 2. Import an ES module

The component imports a module after the first interactive render:

```csharp
_module = await JS.InvokeAsync<IJSObjectReference>(
    "import",
    "./js/interop.js");
```

`wwwroot/js/interop.js` uses ES module exports:

```javascript
export function getBrowserSnapshot() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
}
```

Modules keep related browser functions together and avoid placing many global
functions on `window`. Keep module paths stable and dispose module references
when the component is removed.

## 3. Call JavaScript from C#

The imported module exposes browser operations:

```csharp
BrowserSnapshot snapshot = await _module.InvokeAsync<BrowserSnapshot>(
    "getBrowserSnapshot");
```

The lesson calls JavaScript for several browser APIs:

- `window.innerWidth` and `window.innerHeight` read the viewport;
- `document.getElementById(...).focus()` focuses a DOM element;
- `window.localStorage` saves and loads a note;
- `navigator.clipboard.writeText(...)` copies text.

These calls can fail because a browser may deny permissions, the API may be
unavailable, or the circuit may disconnect. Treat browser calls as asynchronous
operations and handle expected failures at the component boundary.

## 4. Call .NET from JavaScript

JavaScript can invoke a .NET instance method through a
`DotNetObjectReference`:

```csharp
_dotNetReference = DotNetObjectReference.Create(_clockReceiver);
_clockTimerId = await _module.InvokeAsync<int>(
    "startClock",
    _dotNetReference);
```

The receiver exposes the callable method:

```csharp
[JSInvokable]
public Task ReceiveBrowserTime(string currentTime)
{
    TimeReceived?.Invoke(currentTime);
    return Task.CompletedTask;
}
```

The JavaScript timer invokes it by name:

```javascript
dotNetReceiver.invokeMethodAsync(
    "ReceiveBrowserTime",
    new Date().toLocaleTimeString());
```

Instance references are useful when callbacks belong to one component. Static
invokable methods are another option for application-wide callbacks, but they
need a separate way to locate the correct state.

## 5. Synchronization and rendering

The browser clock callback can arrive outside the component's normal event
handler. The component uses `InvokeAsync` before changing UI state:

```csharp
_ = InvokeAsync(() =>
{
    _browserTime = currentTime;
    StateHasChanged();
});
```

This schedules the update on the component's renderer context. Directly
changing component state from an arbitrary callback can produce unsafe or
inconsistent rendering behavior.

## 6. Module and callback disposal

The component implements `IAsyncDisposable` and performs cleanup:

```text
stop the JavaScript timer
unsubscribe the .NET event
dispose the JavaScript module
dispose DotNetObjectReference
```

Without cleanup, a timer can continue calling a component that has already been
removed, and a .NET object reference can keep that component's state alive.
`JSDisconnectedException` is ignored during disposal because a disconnected
circuit has already lost its JavaScript connection.

## 7. Browser APIs are not automatically safe everywhere

Browser APIs have their own security and availability rules:

- clipboard access usually requires a secure context and user permission;
- local storage is browser-local and can be cleared by the user;
- DOM selectors must use controlled IDs and should not concatenate untrusted
  HTML;
- browser state is not a server database or an authorization boundary.

Use normal input validation and authorization on the server even when a value
also comes from a browser API.

## 8. When to use JS interop

Use interop for a browser capability or JavaScript library that the component
framework does not provide directly. Keep the JavaScript surface small and put
it behind a component or service boundary. Avoid using JS interop for ordinary
state and rendering that Blazor already handles well.

## 9. What this lesson leaves for later

The sample does not cover JavaScript module bundling, third-party library
wrappers, authentication-aware browser storage, or WebAssembly-specific
execution. Those choices depend on the selected rendering model and deployment
architecture.

## Run the phase

From the repository root:

```bash
dotnet run --project BlazorLab/BlazorLab.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Open `/interop` to use the browser information, focus, localStorage, clipboard,
and live JavaScript-to-.NET clock examples.
