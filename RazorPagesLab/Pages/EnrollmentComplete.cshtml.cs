using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesLab.Models;
using RazorPagesLab.Services;

namespace RazorPagesLab.Pages;

public sealed class EnrollmentCompleteModel : PageModel
{
    private readonly WorkshopCatalog _catalog;

    public EnrollmentCompleteModel(WorkshopCatalog catalog)
    {
        _catalog = catalog;
    }

    public Workshop? Workshop { get; private set; }

    public IActionResult OnGet(int workshopId)
    {
        Workshop = _catalog.Find(workshopId);
        return Workshop is null ? NotFound() : Page();
    }
}
