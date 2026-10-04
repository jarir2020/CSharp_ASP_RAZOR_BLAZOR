using System.ComponentModel.DataAnnotations;

namespace MvcLab.ViewModels;

public sealed class BookCreateViewModel
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Author { get; set; } = string.Empty;

    [Range(1, 2000)]
    public int Pages { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Summary { get; set; } = string.Empty;
}
