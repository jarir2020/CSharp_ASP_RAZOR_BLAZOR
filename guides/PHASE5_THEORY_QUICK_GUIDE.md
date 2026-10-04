# Phase 5: ASP.NET Core Web API

Phase 5 introduces controller-based APIs. The `AspNetCoreWebApi` project uses a
small in-memory product catalog so the important lessons are HTTP contracts,
model binding, validation, JSON, DTO mapping, and error handling. EF Core will
replace the catalog in Phase 6.

## 1. Controllers and action methods

A controller groups related HTTP actions. `ControllerBase` provides helpers
such as `Ok`, `CreatedAtAction`, `NotFound`, and `NoContent`.

```csharp
[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public ActionResult<ProductResponse> GetById([FromRoute] int id)
    {
        // Find a product and return Ok(...) or NotFound(...).
    }
}
```

`[HttpGet]`, `[HttpPost]`, `[HttpPut]`, and `[HttpDelete]` connect action
methods to HTTP verbs. `ActionResult<T>` documents the normal response type
while still allowing status results such as `NotFound`.

## 2. Model binding

Model binding converts parts of an HTTP request into C# parameters or objects:

- `[FromRoute]` reads route values such as `/api/products/1`.
- `[FromQuery]` reads query values such as `?category=Course`.
- `[FromHeader]` reads a header such as `X-Client-Name`.
- `[FromBody]` deserializes JSON into a request object.
- `[FromForm]` reads URL-encoded or multipart form values.

The Phase 5 controller demonstrates all five forms. Binding is conversion,
not full business validation. The model validation stage still checks required
fields, ranges, lengths, and custom rules.

## 3. DTOs and mapping

A request DTO describes what a client may send. A response DTO describes what
the API promises to return. The domain object used by the catalog is separate:

```text
JSON request -> CreateProductRequest -> ProductDraft -> Product
Product       -> ProductResponse     -> JSON response
```

Returning an EF Core entity directly can expose internal fields, make the API
change when storage changes, and create unwanted update or serialization
behavior. Explicit mapping makes the boundary visible.

## 4. Validation

`[ApiController]` automatically short-circuits an invalid model with a `400`
response. The request DTO uses DataAnnotations such as:

```csharp
[Required]
[StringLength(20, MinimumLength = 3)]
[RegularExpression("^[A-Z0-9-]+$")]
public string Sku { get; init; } = string.Empty;
```

`IValidatableObject` adds a rule that depends on several fields. In the lesson,
Premium products must meet a minimum price. The configured
`InvalidModelStateResponseFactory` returns a `ValidationProblemDetails`
payload with field-level errors.

DataAnnotations are enough for these small examples. FluentValidation is
another option for larger validation rules, but it should solve a real
complexity problem rather than be added automatically.

## 5. JSON options

The project configures `System.Text.Json` to use camelCase property names and
string enum values:

```json
{
  "sku": "API-201",
  "visibility": "Public",
  "metadata": {
    "color": "Green",
    "stock": 4
  }
}
```

The C# source can keep PascalCase properties while JavaScript clients receive
the conventional camelCase format. String enums are easier to understand and
more stable for clients than numeric enum values.

## 6. Consistent API errors

The API uses `ProblemDetails` for errors:

| Situation | Status | Meaning |
| --- | ---: | --- |
| Invalid request fields | 400 | The client input failed validation. |
| Missing product | 404 | The requested resource does not exist. |
| Duplicate SKU | 409 | The request conflicts with existing state. |
| Protected product rule | 422 | The request is valid but violates a business rule. |
| Unexpected exception | 500 | The server failed unexpectedly. |

The custom exception middleware catches known domain exceptions and translates
them into stable responses. Unknown exceptions are logged with the server-side
detail while the client receives a generic `500` response. This prevents
internal exception messages and stack traces from becoming public API output.

Authentication-related `401 Unauthorized` and `403 Forbidden` responses will
be introduced in the security phase after the API contract is established.

## 7. In-memory persistence boundary

The controller depends on `IProductCatalog`, not on the dictionary itself. The
in-memory implementation uses a lock to protect its shared state. In Phase 6,
EF Core will provide the database-backed implementation while the controller's
request and response contract can remain stable.

This project is intentionally not a production database. Its purpose is to
make controller behavior and API contracts easy to observe before introducing
connections, migrations, tracking, and transactions.

## Run the API

From the repository root:

```bash
dotnet run --project AspNetCoreWebApi/AspNetCoreWebApi.csproj
```

Try the endpoints:

```text
GET    /api/products
GET    /api/products/1
GET    /api/products?category=Course&minPrice=50
POST   /api/products
PUT    /api/products/2
DELETE /api/products/2
POST   /api/products/form-note
GET    /api/products/failure-demo
```

Run all tests, including controller integration tests:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

The next phase replaces the in-memory catalog with EF Core entities, a
`DbContext`, relationships, LINQ database queries, migrations, and seeding.
