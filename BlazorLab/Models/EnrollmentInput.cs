using System.ComponentModel.DataAnnotations;

namespace BlazorLab.Models;

public sealed class EnrollmentInput
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(1, 40)]
    public int StudyHoursPerWeek { get; set; }

    [Range(1, 3)]
    public int CourseId { get; set; }
}
