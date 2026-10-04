using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureAspNetCoreApi.Models;
using Xunit;

namespace CSharpCore.Tests;

public sealed class SecureAspNetCoreApiTests : IClassFixture<SecureApiFactory>
{
    private readonly SecureApiFactory _factory;

    public SecureAspNetCoreApiTests(SecureApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Registration_hashes_the_password_and_login_returns_a_bearer_token()
    {
        using HttpClient client = _factory.CreateClient();
        string email = NewEmail();
        const string password = "Pass1234";

        using HttpResponseMessage registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email, password, displayName = "Learner" });
        using HttpResponseMessage loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        JsonElement tokenBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal("Bearer", tokenBody.GetProperty("tokenType").GetString());
        Assert.False(string.IsNullOrWhiteSpace(tokenBody.GetProperty("accessToken").GetString()));

        using IServiceScope scope = _factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        Assert.NotNull(user);
        Assert.NotEqual(password, user.PasswordHash);
    }

    [Fact]
    public async Task Protected_endpoint_requires_authentication()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/secure/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Valid_token_can_read_the_current_user_claims()
    {
        using HttpClient client = _factory.CreateClient();
        string email = NewEmail();
        string token = await RegisterAndLoginAsync(client, email, "Learner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await client.GetAsync("/api/secure/me");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(email, body.GetProperty("email").GetString());
        Assert.Contains("User", body.GetProperty("roles").EnumerateArray().Select(item => item.GetString()));
    }

    [Fact]
    public async Task Role_authorization_returns_forbidden_until_user_has_the_role()
    {
        using HttpClient client = _factory.CreateClient();
        string email = NewEmail();
        const string password = "Pass1234";
        await RegisterAsync(client, email, password, "Role Learner");
        string userToken = await LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        using HttpResponseMessage beforeRole = await client.GetAsync("/api/secure/admin");
        Assert.Equal(HttpStatusCode.Forbidden, beforeRole.StatusCode);

        await AddRoleAsync(email, "Admin");
        string adminToken = await LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using HttpResponseMessage afterRole = await client.GetAsync("/api/secure/admin");
        Assert.Equal(HttpStatusCode.OK, afterRole.StatusCode);
    }

    [Fact]
    public async Task Policy_authorization_requires_a_specific_permission_claim()
    {
        using HttpClient client = _factory.CreateClient();
        string email = NewEmail();
        const string password = "Pass1234";
        await RegisterAsync(client, email, password, "Policy Learner");
        string userToken = await LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        using HttpResponseMessage beforeClaim = await client.PostAsync("/api/secure/course-management", content: null);
        Assert.Equal(HttpStatusCode.Forbidden, beforeClaim.StatusCode);

        await AddPermissionClaimAsync(email, "course.manage");
        string permittedToken = await LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", permittedToken);

        using HttpResponseMessage afterClaim = await client.PostAsync("/api/secure/course-management", content: null);
        Assert.Equal(HttpStatusCode.OK, afterClaim.StatusCode);
    }

    [Fact]
    public async Task Allowed_origin_receives_cors_response_header()
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/secure/me");
        request.Headers.Add("Origin", "http://localhost:3000");
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out IEnumerable<string>? origins));
        Assert.Equal("http://localhost:3000", origins.Single());
    }

    private async Task RegisterAsync(HttpClient client, string email, string password, string displayName)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email, password, displayName });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task<string> RegisterAndLoginAsync(HttpClient client, string email, string displayName)
    {
        const string password = "Pass1234";
        await RegisterAsync(client, email, password, displayName);
        return await LoginAsync(client, email, password);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return body.GetProperty("accessToken").GetString()!;
    }

    private async Task AddRoleAsync(string email, string role)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser user = (await userManager.FindByEmailAsync(email))!;
        IdentityResult result = await userManager.AddToRoleAsync(user, role);
        Assert.True(result.Succeeded);
    }

    private async Task AddPermissionClaimAsync(string email, string permission)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser user = (await userManager.FindByEmailAsync(email))!;
        IdentityResult result = await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("permission", permission));
        Assert.True(result.Succeeded);
    }

    private static string NewEmail()
    {
        return $"user-{Guid.NewGuid():N}@example.test";
    }
}

public sealed class SecureApiFactory : WebApplicationFactory<SecureAspNetCoreApi.Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AuthDbContext>>();
            services.RemoveAll<AuthDbContext>();

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            services.AddSingleton(_connection);
            services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
                options.UseSqlite(serviceProvider.GetRequiredService<SqliteConnection>()));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }
}
