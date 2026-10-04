# Phase 9: ASP.NET Core MVC

Phase 9 introduces the controller-based MVC model through `MvcLab`. The sample
is a small book catalog that separates request handling, application data,
view models, and HTML rendering.

## 1. The MVC request path

The `/Books` request follows this path:

```text
GET /Books
  -> conventional route
  -> BooksController.Index(search)
  -> BookCatalog.Search(search)
  -> BookListViewModel
  -> Views/Books/Index.cshtml
  -> Views/Shared/_Layout.cshtml
  -> HTML response
```

MVC divides responsibility into three cooperating parts:

- **Model**: application data and domain rules. `Book` represents a catalog
  entry and `BookCatalog` owns the sample data.
- **View**: Razor markup that turns a view model into HTML.
- **Controller**: request-facing actions that coordinate the model and view.

The controller should prepare data for the view. The view should focus on
rendering that data and should not contain database queries or business rules.

## 2. Controllers and action methods

`BooksController` inherits from `Controller`, which provides helpers such as
`View`, `NotFound`, `RedirectToAction`, and access to `ModelState`.

```csharp
[HttpGet]
public IActionResult Index(string? search)
{
    // Read data, build a view model, and select the view.
    return View(viewModel);
}
```

An action method is a public method selected by routing and HTTP verb metadata.
An action can return HTML with `View`, a redirect after a successful form post,
or a status result such as `NotFound`.

## 3. Domain models and view models

The `Book` record is a domain model. The catalog owns it and the details page
uses it to calculate a reading estimate.

The list page uses `BookListViewModel`, which contains only the data the page
needs. The create form uses `BookCreateViewModel` with validation attributes.

This separation matters because a form input model is an API boundary. A form
should accept only fields that the user is allowed to submit. Passing a full
domain entity directly to a form can make accidental over-posting easier.

## 4. Views, layouts, and partials

The view files live under `Views/<ControllerName>`:

```text
Views/Books/Index.cshtml
Views/Books/Details.cshtml
Views/Books/Create.cshtml
```

`Views/_ViewStart.cshtml` selects the shared layout. The layout contains the
HTML document shell and navigation. `_BookCard.cshtml` is a partial view that
renders one reusable book card.

The page view loops over the view model and passes one item to the partial:

```razor
@foreach (BookCardViewModel book in Model.Books)
{
    <partial name="_BookCard" model="book" />
}
```

Use a layout for the page shell and a partial for a repeated fragment.

## 5. Conventional and attribute routing

`Program.cs` registers the conventional route:

```csharp
pattern: "{controller=Books}/{action=Index}/{id?}"
```

That route makes these URLs available:

```text
/Books             -> BooksController.Index
/Books/Create      -> BooksController.Create
/Books/Create POST -> BooksController.Create POST action
```

The details action uses an explicit attribute route:

```csharp
[HttpGet("/catalog/{id:int}")]
public IActionResult Details(int id) { ... }
```

The `int` constraint prevents text values from being treated as book IDs. The
two routing styles can coexist. Use the style that matches the application’s
URL design and keep routes readable and stable.

## 6. Model binding and validation

For `GET /Books?search=ASP.NET`, MVC binds the query-string value to the
`search` action parameter. For the create form, MVC binds posted fields into
`BookCreateViewModel`.

DataAnnotations add validation rules:

```csharp
[Required]
[StringLength(120, MinimumLength = 2)]
public string Title { get; set; } = string.Empty;
```

The controller checks `ModelState` before changing the catalog:

```csharp
if (!ModelState.IsValid)
{
    return View(input);
}
```

Returning the same view preserves the submitted values and lets validation Tag
Helpers render the messages beside the invalid fields.

## 7. Forms, antiforgery, and redirects

The create view uses Tag Helpers:

```razor
<form asp-controller="Books" asp-action="Create" method="post">
    <input asp-for="Title" />
    <span asp-validation-for="Title"></span>
</form>
```

The POST action uses `[ValidateAntiForgeryToken]`. The form Tag Helper emits the
matching hidden token. This protects cookie-authenticated form workflows from
cross-site request forgery.

After a valid create request, the controller redirects to the index action:

```text
GET  /Books/Create -> display form
POST /Books/Create -> bind and validate -> save -> redirect
GET  /Books       -> display the updated list
```

This Post-Redirect-Get flow prevents a browser refresh from submitting the
same form again.

## 8. Action filters

`RequestAuditFilter` is registered globally and runs around every MVC action.
It measures the action duration and adds response headers:

```text
X-Mvc-Filter: RequestAuditFilter
X-Mvc-Elapsed-Milliseconds: 3
```

Filters are useful for cross-cutting behavior such as authorization checks,
logging, caching, metrics, and exception handling. Keep filters focused; an
action filter should not quietly contain the business rules of a controller.

## 9. MVC compared with Razor Pages

Razor Pages groups a page’s request handlers and markup around a page model:

```text
Pages/Enroll.cshtml
Pages/Enroll.cshtml.cs
```

MVC groups request actions by controller and views by controller name:

```text
Controllers/BooksController.cs
Views/Books/Index.cshtml
```

Both use Razor views, model binding, validation, Tag Helpers, layouts, and
antiforgery support. Razor Pages is page-focused; MVC is useful when many
actions share a controller-oriented resource boundary or when maintaining an
existing MVC application.

## 10. What this lesson leaves for later

The book catalog is an in-memory singleton so the request path remains easy to
read. A production application would use a database-backed service, scoped
transactions, authorization, structured logging, and a durable error-handling
policy. The next major UI phase introduces Blazor components and their event
and lifecycle model.

## Run the phase

From the repository root:

```bash
dotnet run --project MvcLab/MvcLab.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Open `/Books` for the catalog, `/catalog/1` for attribute-routed details, and
`/Books/Create` for the validated form.
