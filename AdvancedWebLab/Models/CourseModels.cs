namespace AdvancedWebLab.Models;

// A small immutable record keeps the sample catalog easy to query and safe to
// share between requests without exposing mutable application state.
public sealed record Course(
    int Id,
    string Title,
    string Level,
    string Summary);

public sealed record PagedResult<T>(
    int Page,
    int PageSize,
    int TotalItems,
    IReadOnlyList<T> Items);
