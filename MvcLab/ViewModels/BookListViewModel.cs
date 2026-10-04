namespace MvcLab.ViewModels;

public sealed class BookListViewModel
{
    public string SearchTerm { get; init; } = string.Empty;

    public IReadOnlyList<BookCardViewModel> Books { get; init; } = Array.Empty<BookCardViewModel>();

    public bool HasResults => Books.Count > 0;
}
