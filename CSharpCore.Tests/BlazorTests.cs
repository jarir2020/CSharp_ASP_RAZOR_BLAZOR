using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using BlazorLab;
using Xunit;

namespace CSharpCore.Tests;

public sealed class BlazorTests : IClassFixture<BlazorLabFactory>
{
    private readonly BlazorLabFactory _factory;

    public BlazorTests(BlazorLabFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_route_prerenders_blazor_layout_and_components()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Blazor Learning Lab", html);
        Assert.Contains("Learn by changing component state", html);
        Assert.Contains("ASP.NET Core Fundamentals", html);
        Assert.Contains("Increase count", html);
        Assert.Contains("_framework/blazor.web.js", html);
    }

    [Fact]
    public async Task Enrollment_route_prerenders_edit_form_inputs_and_validation_rules()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/enroll");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("EditForm and validation", html);
        Assert.Contains("full-name", html);
        Assert.Contains("study-hours", html);
        Assert.Contains("Choose a course", html);
        Assert.Contains("Submit enrollment", html);
    }

    [Fact]
    public async Task Lifecycle_route_prerenders_the_lifecycle_demo()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/lifecycle");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Component lifecycle", html);
        Assert.Contains("OnInitialized ran", html);
        Assert.Contains("OnParametersSet ran", html);
    }

    [Fact]
    public async Task Interop_route_prerenders_the_interop_controls()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/interop");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("JavaScript interop", html);
        Assert.Contains("Read browser info", html);
        Assert.Contains("interop-note", html);
        Assert.Contains("Copy note", html);
    }

    [Fact]
    public async Task Static_css_asset_is_served()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/app.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task JavaScript_module_is_served()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/js/interop.js");
        string javascript = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("getBrowserSnapshot", javascript);
        Assert.Contains("invokeMethodAsync", javascript);
        Assert.Contains("localStorage", javascript);
    }
}

public sealed class BlazorLabFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // The test server exercises prerendered HTML over HTTP in memory.
        builder.UseEnvironment("Testing");
    }
}
