using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CSharpCore.Tests;

public sealed class AspNetCoreFundamentalsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AspNetCoreFundamentalsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Root_endpoint_returns_configuration_and_environment_information()
    {
        using HttpResponseMessage response = await _client.GetAsync("/");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ASP.NET Core Fundamentals", body.GetProperty("application").GetString());
        Assert.Equal("Welcome to the Phase 4 learning application.", body.GetProperty("message").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("framework").GetString()));
    }

    [Fact]
    public async Task Middleware_adds_a_request_id_header_and_route_returns_a_greeting()
    {
        using HttpResponseMessage response = await _client.GetAsync("/api/hello/Jarir");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Request-Id"));
        Assert.Equal("Hello, Jarir!", body.GetProperty("greeting").GetString());
        Assert.Equal(
            response.Headers.GetValues("X-Request-Id").Single(),
            body.GetProperty("requestId").GetString());
    }

    [Fact]
    public async Task Route_constraint_rejects_a_non_integer_item_id()
    {
        using HttpResponseMessage validResponse = await _client.GetAsync("/api/items/1");
        using HttpResponseMessage invalidRouteResponse = await _client.GetAsync("/api/items/not-an-integer");
        using HttpResponseMessage missingItemResponse = await _client.GetAsync("/api/items/99");

        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, invalidRouteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingItemResponse.StatusCode);
    }

    [Fact]
    public async Task Exception_middleware_returns_a_problem_response()
    {
        using HttpResponseMessage response = await _client.GetAsync("/error-demo");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(500, body.GetProperty("status").GetInt32());
        Assert.Equal("An unexpected error occurred.", body.GetProperty("title").GetString());
    }
}
