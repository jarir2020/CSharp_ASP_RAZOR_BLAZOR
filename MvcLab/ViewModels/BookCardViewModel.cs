namespace MvcLab.ViewModels;

// A small view model keeps the card view independent from the full domain model.
public sealed record BookCardViewModel(
    int Id,
    string Title,
    string Author,
    int Pages,
    string Summary);
