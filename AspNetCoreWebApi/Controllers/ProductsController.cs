using AspNetCoreWebApi.Dtos;
using AspNetCoreWebApi.Models;
using AspNetCoreWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreWebApi.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductCatalog _catalog;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductCatalog catalog,
        ILogger<ProductsController> logger)
    {
        _catalog = catalog;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<ProductResponse>> GetAll(
        [FromQuery] ProductSearchQuery query,
        [FromHeader(Name = "X-Client-Name")] string? clientName)
    {
        _logger.LogInformation(
            "Listing products for client {ClientName} with category {Category}",
            clientName ?? "anonymous",
            query.Category ?? "all");

        IReadOnlyList<ProductResponse> response = _catalog
            .Search(query.Category, query.MinPrice)
            .Select(ProductResponse.FromModel)
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public ActionResult<ProductResponse> GetById([FromRoute] int id)
    {
        Product? product = _catalog.Find(id);

        if (product is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Product not found",
                Detail = $"Product {id} was not found.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(ProductResponse.FromModel(product));
    }

    [HttpPost]
    public ActionResult<ProductResponse> Create([FromBody] CreateProductRequest request)
    {
        Product product = _catalog.Create(request.ToDraft());
        ProductResponse response = ProductResponse.FromModel(product);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPut("{id:int}")]
    public ActionResult<ProductResponse> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductRequest request)
    {
        Product product = _catalog.Update(id, request.ToDraft());
        return Ok(ProductResponse.FromModel(product));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        if (!_catalog.Delete(id))
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Product not found",
                Detail = $"Product {id} was not found.",
                Instance = HttpContext.Request.Path
            });
        }

        return NoContent();
    }

    [HttpPost("form-note")]
    public ActionResult<object> AddFormNote([FromForm] FormNoteRequest request)
    {
        // [FromForm] demonstrates binding application/x-www-form-urlencoded or
        // multipart form data separately from a JSON request body.
        return Ok(new
        {
            received = request.Note.Trim(),
            source = "form"
        });
    }

    [HttpGet("failure-demo")]
    public IActionResult FailureDemo()
    {
        throw new InvalidOperationException("This endpoint demonstrates global API error handling.");
    }
}
