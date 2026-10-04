using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MvcLab;
using Xunit;

namespace CSharpCore.Tests;

public sealed class MvcTests : IClassFixture<MvcLabFactory>
{
    private readonly MvcLabFactory _factory;

    public MvcTests(MvcLabFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Conventional_index_route_renders_layout_partial_and_filter_header()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/Books");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("RequestAuditFilter", response.Headers.GetValues("X-Mvc-Filter").Single());
        Assert.Contains("MVC Lab", html);
        Assert.Contains("MVC book catalog", html);
        Assert.Contains("ASP.NET Core in Practice", html);
        Assert.Contains("href=\"/catalog/1\"", html);
    }

    [Fact]
    public async Task Attribute_details_route_uses_a_view_model_calculation()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/catalog/2");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("C# for Backend Developers", html);
        Assert.Contains("Estimated reading time", html);
        Assert.Contains("9 hour(s)", html);
        Assert.Contains("This is a longer read", html);
    }

    [Fact]
    public async Task Search_query_is_bound_to_the_index_action()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/Books?search=ASP.NET");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("ASP.NET Core in Practice", html);
        Assert.DoesNotContain("C# for Backend Developers", html);
    }

    [Fact]
    public async Task Invalid_create_post_returns_validation_messages()
    {
        using HttpClient client = _factory.CreateClient();
        string formPage = await client.GetStringAsync("/Books/Create");
        string antiforgeryToken = ExtractAntiforgeryToken(formPage);

        using FormUrlEncodedContent form = new(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiforgeryToken,
            ["Title"] = "",
            ["Author"] = "",
            ["Pages"] = "0",
            ["Summary"] = "short"
        });

        using HttpResponseMessage response = await client.PostAsync("/Books/Create", form);
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The Title field is required.", html);
        Assert.Contains("The Author field is required.", html);
        Assert.Contains("The field Pages must be between 1 and 2000.", html);
        Assert.Contains("The field Summary must be a string with a minimum length of 10", html);
    }

    [Fact]
    public async Task Valid_create_post_redirects_to_the_index_action()
    {
        using HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        string formPage = await client.GetStringAsync("/Books/Create");
        string antiforgeryToken = ExtractAntiforgeryToken(formPage);

        using FormUrlEncodedContent form = new(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiforgeryToken,
            ["Title"] = "Testing MVC Forms",
            ["Author"] = "Demo Author",
            ["Pages"] = "120",
            ["Summary"] = "A sufficiently long summary for the MVC form test."
        });

        using HttpResponseMessage response = await client.PostAsync("/Books/Create", form);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Books", response.Headers.Location?.OriginalString);
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

public sealed class MvcLabFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // TestServer handles HTTP in memory, so HTTPS redirection is disabled.
        builder.UseEnvironment("Testing");
    }
}
