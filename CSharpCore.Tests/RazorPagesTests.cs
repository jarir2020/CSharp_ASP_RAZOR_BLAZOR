using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RazorPagesLab;
using Xunit;

namespace CSharpCore.Tests;

public sealed class RazorPagesTests : IClassFixture<RazorPagesFactory>
{
    private readonly RazorPagesFactory _factory;

    public RazorPagesTests(RazorPagesFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Index_renders_layout_partial_and_tag_helper_link()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Razor Pages Lab", html);
        Assert.Contains("Explore upcoming workshops", html);
        Assert.Contains("ASP.NET Core Request Pipeline", html);
        Assert.Contains("href=\"/Enroll/1\"", html);
        Assert.Contains("This workshop is full.", html);
    }

    [Fact]
    public async Task Enroll_page_renders_bound_inputs_and_antiforgery_token()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/Enroll/1");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("name=\"Input.FullName\"", html);
        Assert.Contains("name=\"Input.Email\"", html);
        Assert.Contains("name=\"__RequestVerificationToken\"", html);
    }

    [Fact]
    public async Task Invalid_post_returns_the_page_with_validation_errors()
    {
        using HttpClient client = _factory.CreateClient();
        string formPage = await client.GetStringAsync("/Enroll/1");
        string antiforgeryToken = ExtractAntiforgeryToken(formPage);

        using FormUrlEncodedContent form = new(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiforgeryToken,
            ["WorkshopId"] = "1",
            ["Input.FullName"] = "",
            ["Input.Email"] = "not-an-email"
        });

        using HttpResponseMessage response = await client.PostAsync("/Enroll/1", form);
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The FullName field is required.", html);
        Assert.Contains("The Email field is not a valid e-mail address.", html);
    }

    [Fact]
    public async Task Valid_post_redirects_to_the_confirmation_page()
    {
        using HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        string formPage = await client.GetStringAsync("/Enroll/1");
        string antiforgeryToken = ExtractAntiforgeryToken(formPage);

        using FormUrlEncodedContent form = new(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiforgeryToken,
            ["WorkshopId"] = "1",
            ["Input.FullName"] = "Amina Rahman",
            ["Input.Email"] = "amina@example.test"
        });

        using HttpResponseMessage response = await client.PostAsync("/Enroll/1", form);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/EnrollmentComplete/1", response.Headers.Location?.OriginalString);
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        Match match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "The form did not contain an antiforgery token.");
        return match.Groups[1].Value;
    }
}

public sealed class RazorPagesFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // TestServer handles HTTP in memory, so the app skips HTTPS redirection
        // while the integration tests exercise page rendering and POST logic.
        builder.UseEnvironment("Testing");
    }
}
