using MvcLab.Models;

namespace MvcLab.Services;

public sealed class BookCatalog
{
    private readonly object _sync = new();
    private readonly List<Book> _books = new()
    {
        new(
            1,
            "ASP.NET Core in Practice",
            "Mina Rahman",
            280,
            "A guided tour through requests, middleware, controllers, and views."),
        new(
            2,
            "C# for Backend Developers",
            "Arif Hasan",
            360,
            "A practical bridge from dynamic-language backend work to modern C#."),
        new(
            3,
            "Reliable Web Forms",
            "Nadia Karim",
            180,
            "A compact guide to validation, binding, errors, and safe form handling.")
    };

    private int _nextId = 4;

    public IReadOnlyList<Book> Search(string? searchTerm)
    {
        lock (_sync)
        {
            IEnumerable<Book> query = _books;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedTerm = searchTerm.Trim();
                query = query.Where(book =>
                    book.Title.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
                    || book.Author.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderBy(book => book.Title).ToArray();
        }
    }

    public Book? Find(int id)
    {
        lock (_sync)
        {
            return _books.FirstOrDefault(book => book.Id == id);
        }
    }

    public Book Add(string title, string author, int pages, string summary)
    {
        lock (_sync)
        {
            Book book = new(_nextId++, title.Trim(), author.Trim(), pages, summary.Trim());
            _books.Add(book);
            return book;
        }
    }
}
