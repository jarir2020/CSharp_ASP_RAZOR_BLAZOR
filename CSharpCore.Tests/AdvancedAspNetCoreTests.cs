using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CSharpCore.Tests;

public sealed class AdvancedAspNetCoreTests : IClassFixture<AdvancedAspNetCoreFactory>
{
    private readonly AdvancedAspNetCoreFactory _factory;

    public AdvancedAspNetCoreTests(AdvancedAspNetCoreFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Options_configuration_and_advanced_di_are_available()
    {
        using HttpClient client = _factory.CreateClient();

        JsonElement config = await client.GetFromJsonAsync<JsonElement>("/api/config");
        JsonElement di = await client.GetFromJsonAsync<JsonElement>("/api/di");

        Assert.Equal("Advanced ASP.NET Core Lab", config.GetProperty("displayName").GetString());
        Assert.Equal("course-refresh", config.GetProperty("queueName").GetString());
        Assert.Equal("system-keyed-clock", di.GetProperty("systemClock").GetString());
        Assert.Equal("fixed-keyed-clock", di.GetProperty("fixedClock").GetString());
        Assert.Contains("CatalogItem", di.GetProperty("envelope").GetString());
    }

    [Fact]
    public async Task Memory_cache_serves_the_second_catalog_request()
    {
        using HttpClient client = _factory.CreateClient();

        await client.GetFromJsonAsync<JsonElement>("/api/cache/memory");
        JsonElement second = await client.GetFromJsonAsync<JsonElement>("/api/cache/memory");

        Assert.Equal("memory", second.GetProperty("source").GetString());
    }

    [Fact]
    public async Task Output_cache_reuses_a_public_get_response()
    {
        using HttpClient client = _factory.CreateClient();
        string first = await client.GetStringAsync("/api/cache/output?sample=phase12");
        string second = await client.GetStringAsync("/api/cache/output?sample=phase12");

        Assert.Equal(first, second);
    }

    [Fact]
    public async Task Response_cache_endpoint_sets_http_cache_headers()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/cache/response");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("public,max-age=5", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Queued_job_reaches_completed_state()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage queued = await client.PostAsync("/api/jobs", content: null);
        JsonElement queuedBody = await queued.Content.ReadFromJsonAsync<JsonElement>();
        Guid id = queuedBody.GetProperty("id").GetGuid();

        string finalState = "Pending";
        for (int attempt = 0; attempt < 20; attempt++)
        {
            JsonElement statusBody = await client.GetFromJsonAsync<JsonElement>($"/api/jobs/{id}");
            finalState = statusBody.GetProperty("state").GetString() ?? "Missing";

            if (finalState is "Completed" or "Failed")
            {
                break;
            }

            await Task.Delay(25);
        }

        Assert.Equal(HttpStatusCode.Accepted, queued.StatusCode);
        Assert.Equal("Completed", finalState);
    }
}

public sealed class AdvancedAspNetCoreFactory : WebApplicationFactory<AdvancedAspNetCoreLab.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
