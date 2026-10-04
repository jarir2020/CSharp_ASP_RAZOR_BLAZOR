using Microsoft.AspNetCore.Mvc;
using MvcLab.Models;
using MvcLab.Services;
using MvcLab.ViewModels;

namespace MvcLab.Controllers;

public sealed class BooksController : Controller
{
    private readonly BookCatalog _catalog;

    public BooksController(BookCatalog catalog)
    {
        _catalog = catalog;
    }

    [HttpGet]
    public IActionResult Index(string? search)
    {
        IReadOnlyList<BookCardViewModel> books = _catalog
            .Search(search)
            .Select(book => new BookCardViewModel(
                book.Id,
                book.Title,
                book.Author,
                book.Pages,
                book.Summary))
            .ToArray();

        return View(new BookListViewModel
        {
            SearchTerm = search?.Trim() ?? string.Empty,
            Books = books
        });
    }

    // This explicit route demonstrates attribute routing alongside the
    // conventional {controller}/{action}/{id?} route used by Index and Create.
    [HttpGet("/catalog/{id:int}")]
    public IActionResult Details(int id)
    {
        Book? book = _catalog.Find(id);

        if (book is null)
        {
            return NotFound();
        }

        return View(new BookDetailsViewModel
        {
            Book = book
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new BookCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(BookCreateViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        _catalog.Add(input.Title, input.Author, input.Pages, input.Summary);
        return RedirectToAction(nameof(Index));
    }
}
