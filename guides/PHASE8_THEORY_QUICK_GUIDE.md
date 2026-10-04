# Phase 8: Razor and Razor Pages

Phase 8 introduces server-rendered HTML with Razor syntax and the Razor Pages
programming model. The example application is `RazorPagesLab`.

## 1. Razor, Razor Pages, and Blazor

These names are related but describe different layers:

- **Razor syntax** is the template syntax that mixes HTML with C# expressions.
- **Razor Pages** is a server-rendered page model built on ASP.NET Core MVC.
- **Blazor** is a component-based UI framework. Blazor components can also use
  Razor syntax, but Blazor is covered later in this course.

Razor syntax is the small piece shared by the other two concepts:

```razor
<h1>Hello @Model.Heading</h1>
```

The `@` character switches from ordinary HTML into a C# expression. Razor
returns to HTML after the expression is complete.

## 2. The Razor Pages request path

The `/` page in this phase follows this path:

```text
GET /
  -> IndexModel.OnGet()
  -> WorkshopCatalog.GetAll()
  -> Pages/Index.cshtml
  -> Pages/_ViewStart.cshtml
  -> Pages/Shared/_Layout.cshtml
  -> HTML response
```

The `.cshtml` file controls the markup. The matching `.cshtml.cs` file is the
PageModel that receives dependencies and handles requests. This keeps request
logic out of the HTML template.

## 3. Razor expressions, conditions, and loops

`Pages/Index.cshtml` uses ordinary C# control flow inside an HTML template:

```razor
@if (Model.Workshops.Count == 0)
{
    <p>No workshops are scheduled.</p>
}
else
{
    @foreach (Workshop workshop in Model.Workshops)
    {
        <partial name="_WorkshopCard" model="workshop" />
    }
}
```

The page model prepares data. The view decides how that data is represented as
HTML. Avoid putting database queries and complicated business rules directly in
`.cshtml` files.

## 4. Layouts and partial views

`Pages/_ViewStart.cshtml` selects the shared layout for pages below it:

```razor
@{
    Layout = "_Layout";
}
```

`Pages/Shared/_Layout.cshtml` provides the document shell, navigation, CSS link,
and footer. Individual pages provide only their content through
`@RenderBody()`.

The `_WorkshopCard.cshtml` partial receives one `Workshop` model and renders a
reusable card. Partial views are useful when a repeated visual fragment needs
its own small template. A layout is the larger page shell; a partial is a
reusable fragment inside a page.

## 5. Tag Helpers

`Pages/_ViewImports.cshtml` enables the built-in MVC Tag Helpers:

```razor
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

The link below is written with page and route information rather than a hard-
coded URL:

```razor
<a asp-page="/Enroll" asp-route-workshopId="@Model.Id">Enroll</a>
```

The `asp-page` and `asp-route-workshopId` attributes are transformed into a
normal HTML link such as `/Enroll/1`. The form Tag Helpers similarly generate
field names, IDs, labels, validation attributes, and an antiforgery token.

## 6. PageModel handlers and binding

The enrollment page declares the route value and form object:

```csharp
[BindProperty(SupportsGet = true)]
public int WorkshopId { get; set; }

[BindProperty]
public EnrollmentInput Input { get; set; } = new();
```

`OnGet` loads the workshop for the initial page request. `OnPost` receives the
submitted form and checks `ModelState` before changing application state.

The `SupportsGet` option allows `WorkshopId` to be populated from the route on
the initial GET. A normal `[BindProperty]` is appropriate for posted form
values. Use binding deliberately: accept only the fields a page is allowed to
change.

## 7. Forms and validation

`EnrollmentInput` uses DataAnnotations:

```csharp
[Required]
[EmailAddress]
[StringLength(200)]
public string Email { get; set; } = string.Empty;
```

The model binder converts request fields into the C# object. Validation adds
errors to `ModelState`. The validation Tag Helpers display those errors beside
the matching fields:

```razor
<input asp-for="Input.Email" />
<span asp-validation-for="Input.Email"></span>
```

The server must always validate. Client-side validation can improve feedback,
but browser validation is not a security boundary because clients can send
requests without using the browser UI.

## 8. Antiforgery and the POST-Redirect-GET pattern

The form Tag Helper emits an antiforgery token. ASP.NET Core checks that token
when the form is posted, which helps protect cookie-authenticated applications
from cross-site request forgery.

After a valid enrollment, the page redirects to `/EnrollmentComplete/1`:

```text
GET /Enroll/1
  -> display form
POST /Enroll/1
  -> bind and validate
  -> reserve a seat
  -> redirect
GET /EnrollmentComplete/1
  -> display confirmation
```

This is the Post-Redirect-Get pattern. Refreshing the confirmation page does
not submit the original form again.

## 9. What this lesson leaves for later

The catalog is an in-memory singleton so the example stays small. A production
application would use a database, a scoped application service, concurrency
constraints, authentication, authorization, structured logging, and a proper
error-handling strategy. Phase 9 covers MVC concepts, and the later Blazor
phase covers component rendering and event-driven UI.

## Run the phase

From the repository root:

```bash
dotnet run --project RazorPagesLab/RazorPagesLab.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Open the URL printed by ASP.NET Core and visit `/` for the workshop list. The
enrollment form is available at `/Enroll/1`; workshop 2 intentionally shows the
conditional full-state branch.
