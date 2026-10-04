using System.ComponentModel.DataAnnotations;

namespace RazorPagesLab.Models;

public sealed class EnrollmentInput
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
}
