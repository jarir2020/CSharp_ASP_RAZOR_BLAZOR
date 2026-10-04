using System.ComponentModel.DataAnnotations;

namespace AdvancedAspNetCoreLab.Configuration;

public sealed class CoursePlatformOptions
{
    public const string SectionName = "CoursePlatform";

    [Required]
    [MinLength(3)]
    public string DisplayName { get; set; } = string.Empty;

    [Range(1, 300)]
    public int MemoryCacheSeconds { get; set; }

    [Range(1, 300)]
    public int OutputCacheSeconds { get; set; }

    [Required]
    [MinLength(3)]
    public string QueueName { get; set; } = string.Empty;
}
