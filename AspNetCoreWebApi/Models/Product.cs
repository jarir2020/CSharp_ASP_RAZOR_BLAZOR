namespace AspNetCoreWebApi.Models;

public enum ProductVisibility
{
    Public,
    Internal
}

/// <summary>
/// This is the in-memory domain object. The API returns a separate response
/// DTO instead of exposing this storage shape directly.
/// </summary>
public sealed class Product
{
    public int Id { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public ProductVisibility Visibility { get; init; }

    public ProductMetadata Metadata { get; init; } = new();
}

public sealed class ProductMetadata
{
    public string Color { get; init; } = string.Empty;

    public int Stock { get; init; }
}

public sealed class ProductDraft
{
    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public ProductVisibility Visibility { get; init; }

    public ProductMetadata Metadata { get; init; } = new();
}

public sealed class ProductConflictException : Exception
{
    public ProductConflictException(string message)
        : base(message)
    {
    }
}

public sealed class ProductDomainException : Exception
{
    public ProductDomainException(string message)
        : base(message)
    {
    }
}
