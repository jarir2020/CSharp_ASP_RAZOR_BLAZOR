using MvcLab.Models;

namespace MvcLab.ViewModels;

public sealed class BookDetailsViewModel
{
    public required Book Book { get; init; }

    // A view-specific calculation belongs in the view model, not in the view.
    public int EstimatedReadingHours => Math.Max(1, (int)Math.Ceiling(Book.Pages / 40d));

    public bool IsLongRead => Book.Pages >= 300;
}
