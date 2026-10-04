using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecureAspNetCoreApi.Controllers;

[ApiController]
[Route("api/secure")]
[Authorize]
public sealed class SecureController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            displayName = User.FindFirstValue("display_name"),
            roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray()
        });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "Admin access granted." });
    }

    [HttpPost("course-management")]
    [Authorize(Policy = "CourseManagement")]
    public IActionResult CourseManagement()
    {
        return Ok(new { message = "Course management permission granted." });
    }
}
