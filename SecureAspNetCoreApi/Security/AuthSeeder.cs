using Microsoft.AspNetCore.Identity;
using SecureAspNetCoreApi.Models;

namespace SecureAspNetCoreApi.Security;

public static class AuthSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (string role in new[] { "User", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Unable to create role {role}: {string.Join(", ", result.Errors.Select(error => error.Code))}");
                }
            }
        }
    }
}
