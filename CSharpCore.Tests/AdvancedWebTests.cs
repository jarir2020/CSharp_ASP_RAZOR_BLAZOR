using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AdvancedWebLab;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CSharpCore.Tests;

public sealed class AdvancedWebTests : IClassFixture<AdvancedWebLabFactory>
{
    private readonly AdvancedWebLabFactory _factory;

    public AdvancedWebTests(AdvancedWebLabFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OpenApi_contract_and_health_endpoints_are_available()
    {
        using HttpClient client = _factory.CreateClient();

        JsonElement openApi = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        using HttpResponseMessage health = await client.GetAsync("/health");

        Assert.Equal("3.0.3", openApi.GetProperty("openapi").GetString());
        Assert.True(openApi.GetProperty("paths").GetProperty("/api/v1/courses").ValueKind != JsonValueKind.Null);
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal("Healthy", await health.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Course_api_supports_pagination_filtering_sorting_and_versioning()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage v1Response = await client.GetAsync(
            "/api/v1/courses?page=1&pageSize=2&sort=title");
        JsonElement v1 = await v1Response.Content.ReadFromJsonAsync<JsonElement>();

        using HttpResponseMessage v2Response = await client.GetAsync(
            "/api/v2/courses?search=advanced");
        JsonElement v2 = await v2Response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("1.0", v1Response.Headers.GetValues("X-Api-Version").Single());
        Assert.Equal(2, v1.GetProperty("items").GetArrayLength());
        Assert.Equal("Advanced", v2.GetProperty("items")[0].GetProperty("level").GetString());
        Assert.Equal("2.0", v2Response.Headers.GetValues("X-Api-Version").Single());
        Assert.True(v2.GetProperty("items")[0].TryGetProperty("durationMinutes", out _));
    }

    [Fact]
    public async Task File_upload_consumes_a_valid_multipart_file()
    {
        using HttpClient client = _factory.CreateClient();
        using MultipartFormDataContent form = new();
        using StringContent contents = new("course notes");
        form.Add(contents, "file", "notes.txt");

        using HttpResponseMessage response = await client.PostAsync("/api/v1/files", form);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("notes.txt", body.GetProperty("fileName").GetString());
        Assert.Equal(".txt", body.GetProperty("extension").GetString());
    }

    [Fact]
    public async Task Streaming_endpoint_returns_newline_delimited_items()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/v1/stream");
        string[] lines = (await response.Content.ReadAsStringAsync())
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/x-ndjson", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(4, lines.Length);
    }

    [Fact]
    public async Task Fixed_window_rate_limit_rejects_the_third_request()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage first = await client.GetAsync("/api/v1/limited");
        using HttpResponseMessage second = await client.GetAsync("/api/v1/limited");
        using HttpResponseMessage third = await client.GetAsync("/api/v1/limited");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }

    [Fact]
    public async Task SignalR_negotiate_endpoint_is_mapped()
    {
        using HttpClient client = _factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/hubs/courses/negotiate?negotiateVersion=1",
            content: null);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("connectionId").GetString()?.Length > 0);
    }
}

public sealed class AdvancedWebLabFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
