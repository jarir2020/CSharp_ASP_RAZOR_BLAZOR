namespace AdvancedAspNetCoreLab.Models;

public sealed record CatalogItem(
    int Id,
    string Title,
    string Level);

public sealed record CatalogSnapshot(
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<CatalogItem> Items,
    string Source);
