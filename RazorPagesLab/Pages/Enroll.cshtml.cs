using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesLab.Models;
using RazorPagesLab.Services;

namespace RazorPagesLab.Pages;

public sealed class EnrollModel : PageModel
{
    private readonly WorkshopCatalog _catalog;

    public EnrollModel(WorkshopCatalog catalog)
    {
        _catalog = catalog;
    }

    [BindProperty(SupportsGet = true)]
    public int WorkshopId { get; set; }

    [BindProperty]
    public EnrollmentInput Input { get; set; } = new();

    public Workshop? Workshop { get; private set; }

    public IActionResult OnGet()
    {
        Workshop = _catalog.Find(WorkshopId);
        return Workshop is null ? NotFound() : Page();
    }

    // OnPost runs after the form submits. ModelState contains validation errors
    // created from the DataAnnotations on EnrollmentInput.
    public IActionResult OnPost()
    {
        Workshop = _catalog.Find(WorkshopId);

        if (Workshop is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!_catalog.TryEnroll(WorkshopId))
        {
            ModelState.AddModelError(string.Empty, "This workshop has no seats left.");
            return Page();
        }

        // Redirect after a successful POST prevents accidental duplicate form
        // submissions when the user refreshes the confirmation page.
        return RedirectToPage("/EnrollmentComplete", new { workshopId = WorkshopId });
    }
}
