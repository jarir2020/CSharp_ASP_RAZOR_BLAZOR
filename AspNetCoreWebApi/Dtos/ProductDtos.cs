using System.ComponentModel.DataAnnotations;
using AspNetCoreWebApi.Models;

namespace AspNetCoreWebApi.Dtos;

public sealed class ProductSearchQuery
{
    public string? Category { get; init; }

    [Range(0, 1_000_000)]
    public decimal? MinPrice { get; init; }
}

/// <summary>
/// Request DTOs describe the input contract. They are separate from the
/// in-memory Product model so storage changes do not silently change the API.
/// </summary>
public class CreateProductRequest : IValidatableObject
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [RegularExpression("^[A-Z0-9-]+$")]
    public string Sku { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string Category { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; init; }

    public ProductVisibility Visibility { get; init; } = ProductVisibility.Public;

    [Required]
    public ProductMetadataRequest Metadata { get; init; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // DataAnnotations handle field-level rules. IValidatableObject handles
        // a rule that depends on more than one field.
        if (string.Equals(Category, "Premium", StringComparison.OrdinalIgnoreCase) && Price < 100m)
        {
            yield return new ValidationResult(
                "Premium products must cost at least 100.",
                new[] { nameof(Category), nameof(Price) });
        }
    }

    public ProductDraft ToDraft()
    {
        return new ProductDraft
        {
            Sku = Sku.Trim(),
            Name = Name.Trim(),
            Category = Category.Trim(),
            Price = Price,
            Visibility = Visibility,
            Metadata = Metadata.ToModel()
        };
    }
}

public sealed class UpdateProductRequest : CreateProductRequest
{
}

public sealed class ProductMetadataRequest
{
    [Required]
    [StringLength(30, MinimumLength = 2)]
    public string Color { get; init; } = string.Empty;

    [Range(0, 1_000_000)]
    public int Stock { get; init; }

    public ProductMetadata ToModel()
    {
        return new ProductMetadata
        {
            Color = Color.Trim(),
            Stock = Stock
        };
    }
}

public sealed class ProductResponse
{
    public int Id { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public ProductVisibility Visibility { get; init; }

    public ProductMetadataResponse Metadata { get; init; } = new();

    public static ProductResponse FromModel(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Category = product.Category,
            Price = product.Price,
            Visibility = product.Visibility,
            Metadata = new ProductMetadataResponse
            {
                Color = product.Metadata.Color,
                Stock = product.Metadata.Stock
            }
        };
    }
}

public sealed class ProductMetadataResponse
{
    public string Color { get; init; } = string.Empty;

    public int Stock { get; init; }
}

public sealed class FormNoteRequest
{
    [Required]
    [StringLength(200)]
    public string Note { get; init; } = string.Empty;
}
