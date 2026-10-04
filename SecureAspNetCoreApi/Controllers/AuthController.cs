using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SecureAspNetCoreApi.Models;
using SecureAspNetCoreApi.Security;

namespace SecureAspNetCoreApi.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtTokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        ApplicationUser user = new()
        {
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim()
        };

        IdentityResult result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            ValidationProblemDetails problem = new(
                result.Errors
                    .GroupBy(error => error.Code)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.Description).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Registration failed."
            };

            return BadRequest(problem);
        }

        await _userManager.AddToRoleAsync(user, "User");

        return Created($"/api/auth/users/{user.Id}", new
        {
            userId = user.Id,
            email = user.Email,
            displayName = user.DisplayName
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);

        // Do not reveal whether the email or password was the invalid part.
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials."
            });
        }

        TokenResponse token = await _tokenService.CreateAccessTokenAsync(user);
        return Ok(token);
    }

    [HttpGet("claims")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Claims()
    {
        return Ok(User.Claims.Select(claim => new
        {
            type = claim.Type,
            value = claim.Value
        }));
    }
}
