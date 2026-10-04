using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SecureAspNetCoreApi.Models;

namespace SecureAspNetCoreApi.Security;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "secure-course-api";

    public string Audience { get; init; } = "secure-course-client";

    public string Key { get; init; } = string.Empty;

    public int LifetimeMinutes { get; init; } = 30;

    public static JwtOptions FromConfiguration(IConfiguration configuration)
    {
        string? configuredKey = configuration["Jwt:Key"];

        // A local demo can start without a committed secret. The generated key
        // is process-local; production must provide a durable secret through a
        // secret manager or environment configuration.
        string key = string.IsNullOrWhiteSpace(configuredKey)
            ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            : configuredKey;

        return new JwtOptions
        {
            Issuer = configuration["Jwt:Issuer"] ?? "secure-course-api",
            Audience = configuration["Jwt:Audience"] ?? "secure-course-client",
            Key = key,
            LifetimeMinutes = int.TryParse(configuration["Jwt:LifetimeMinutes"], out int minutes)
                ? Math.Clamp(minutes, 5, 120)
                : 30
        };
    }
}

public sealed class JwtTokenService
{
    private readonly JwtOptions _options;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(
        JwtOptions options,
        UserManager<ApplicationUser> userManager)
    {
        _options = options;
        _userManager = userManager;
    }

    public async Task<TokenResponse> CreateAccessTokenAsync(ApplicationUser user)
    {
        List<Claim> claims = new()
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("display_name", user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        IEnumerable<string> roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        IEnumerable<Claim> userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.LifetimeMinutes);
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
