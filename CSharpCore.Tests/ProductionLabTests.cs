using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductionLab;
using Xunit;

namespace CSharpCore.Tests;

public sealed class ProductionLabTests : IClassFixture<ProductionLabFactory>
{
    private readonly ProductionLabFactory _factory;

    public ProductionLabTests(ProductionLabFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Liveness_and_readiness_endpoints_report_healthy()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage live = await client.GetAsync("/health/live");
        using HttpResponseMessage ready = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
        Assert.Equal("Healthy", await ready.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Safe_configuration_reports_secret_presence_without_returning_secret()
    {
        using HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/config/safe");
        request.Headers.Add("X-Correlation-ID", "phase15-test-correlation");

        using HttpResponseMessage response = await client.SendAsync(request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Production ASP.NET Core Lab", body.GetProperty("serviceName").GetString());
        Assert.False(body.GetProperty("secretConfigured").GetBoolean());
        Assert.Equal(
            "phase15-test-correlation",
            response.Headers.GetValues("X-Correlation-ID").Single());
        Assert.DoesNotContain("ExternalApiKey", body.ToString());
    }

    [Fact]
    public async Task Runtime_endpoint_returns_process_diagnostics()
    {
        using HttpClient client = _factory.CreateClient();

        JsonElement body = await client.GetFromJsonAsync<JsonElement>("/api/runtime");

        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("framework").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("operatingSystem").GetString()));
        Assert.True(body.GetProperty("processId").GetInt32() > 0);
    }
}

public sealed class ProductionLabFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // The test host uses HTTP in memory and does not require a production
        // secret because the readiness rule is stricter only in Production.
        builder.UseEnvironment("Testing");
    }
}
