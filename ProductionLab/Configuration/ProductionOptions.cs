using System.ComponentModel.DataAnnotations;

namespace ProductionLab.Configuration;

public sealed class ProductionOptions
{
    public const string SectionName = "Production";

    [Required]
    public string ServiceName { get; init; } = "Production ASP.NET Core Lab";

    [Range(5, 300)]
    public int ShutdownTimeoutSeconds { get; init; } = 30;

    // This value must come from an environment variable or secret manager in
    // Production. It is intentionally absent from appsettings.Production.json.
    public string? ExternalApiKey { get; init; }

    public bool SecretConfigured => !string.IsNullOrWhiteSpace(ExternalApiKey);
}
