namespace MvcLab.Models;

// This domain model represents data that the catalog owns.
public sealed record Book(
    int Id,
    string Title,
    string Author,
    int Pages,
    string Summary);
