using AspNetCoreWebApi.Models;

namespace AspNetCoreWebApi.Services;

public interface IProductCatalog
{
    IReadOnlyList<Product> Search(string? category, decimal? minimumPrice);

    Product? Find(int id);

    Product Create(ProductDraft draft);

    Product Update(int id, ProductDraft draft);

    bool Delete(int id);
}

/// <summary>
/// This temporary repository uses a locked dictionary. Phase 6 will replace it
/// with EF Core without changing the controller's HTTP contract.
/// </summary>
public sealed class InMemoryProductCatalog : IProductCatalog
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<int, Product> _products = new()
    {
        [1] = new Product
        {
            Id = 1,
            Sku = "CSHARP-101",
            Name = "C# Fundamentals",
            Category = "Course",
            Price = 100m,
            Visibility = ProductVisibility.Public,
            Metadata = new ProductMetadata { Color = "Blue", Stock = 10 }
        }
    };

    private int _nextId = 2;

    public IReadOnlyList<Product> Search(string? category, decimal? minimumPrice)
    {
        lock (_syncRoot)
        {
            return _products.Values
                .Where(product => string.IsNullOrWhiteSpace(category)
                    || string.Equals(product.Category, category.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(product => !minimumPrice.HasValue || product.Price >= minimumPrice.Value)
                .OrderBy(product => product.Id)
                .ToList();
        }
    }

    public Product? Find(int id)
    {
        lock (_syncRoot)
        {
            return _products.GetValueOrDefault(id);
        }
    }

    public Product Create(ProductDraft draft)
    {
        lock (_syncRoot)
        {
            EnsureSkuIsAvailable(draft.Sku, ignoredId: null);

            Product product = new()
            {
                Id = _nextId++,
                Sku = draft.Sku,
                Name = draft.Name,
                Category = draft.Category,
                Price = draft.Price,
                Visibility = draft.Visibility,
                Metadata = draft.Metadata
            };

            _products.Add(product.Id, product);
            return product;
        }
    }

    public Product Update(int id, ProductDraft draft)
    {
        lock (_syncRoot)
        {
            if (!_products.ContainsKey(id))
            {
                throw new KeyNotFoundException($"Product {id} was not found.");
            }

            EnsureSkuIsAvailable(draft.Sku, ignoredId: id);

            Product updated = new()
            {
                Id = id,
                Sku = draft.Sku,
                Name = draft.Name,
                Category = draft.Category,
                Price = draft.Price,
                Visibility = draft.Visibility,
                Metadata = draft.Metadata
            };

            _products[id] = updated;
            return updated;
        }
    }

    public bool Delete(int id)
    {
        lock (_syncRoot)
        {
            if (id == 1)
            {
                throw new ProductDomainException("The seeded learning product cannot be deleted.");
            }

            return _products.Remove(id);
        }
    }

    private void EnsureSkuIsAvailable(string sku, int? ignoredId)
    {
        bool duplicate = _products.Values.Any(product =>
            product.Id != ignoredId
            && string.Equals(product.Sku, sku, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            throw new ProductConflictException($"The SKU '{sku}' is already in use.");
        }
    }
}
