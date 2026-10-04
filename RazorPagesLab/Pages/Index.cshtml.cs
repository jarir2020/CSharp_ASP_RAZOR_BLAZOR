using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesLab.Models;
using RazorPagesLab.Services;

namespace RazorPagesLab.Pages;

public sealed class IndexModel : PageModel
{
    private readonly WorkshopCatalog _catalog;

    public IndexModel(WorkshopCatalog catalog)
    {
        _catalog = catalog;
    }

    public string Heading => "Explore upcoming workshops";

    public IReadOnlyList<Workshop> Workshops { get; private set; } = Array.Empty<Workshop>();

    // OnGet runs when the browser requests the page with HTTP GET.
    public void OnGet()
    {
        Workshops = _catalog.GetAll();
    }
}
