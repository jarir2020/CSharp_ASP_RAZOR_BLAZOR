using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AspNetCoreWebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CSharpCore.Tests;

public sealed class AspNetCoreWebApiTests : IClassFixture<WebApplicationFactory<AspNetCoreWebApi.Program>>
{
    private readonly HttpClient _client;

    public AspNetCoreWebApiTests(WebApplicationFactory<AspNetCoreWebApi.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_binds_query_and_header_values_and_returns_response_dto()
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/products?category=Course&minPrice=50");
        request.Headers.Add("X-Client-Name", "phase5-tests");

        using HttpResponseMessage response = await _client.SendAsync(request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        bool containsSeededProduct = body.EnumerateArray().Any(product =>
            product.GetProperty("name").GetString() == "C# Fundamentals"
            && product.GetProperty("visibility").GetString() == "Public"
            && product.GetProperty("metadata").GetProperty("color").GetString() == "Blue");

        Assert.True(containsSeededProduct);
    }

    [Fact]
    public async Task Post_binds_json_body_and_returns_created_location()
    {
        object request = new
        {
            sku = "API-201",
            name = "Web API Course",
            category = "Course",
            price = 125.50m,
            visibility = "Public",
            metadata = new { color = "Green", stock = 4 }
        };

        using HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("API-201", body.GetProperty("sku").GetString());
        Assert.Equal(125.50m, body.GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task Invalid_json_returns_validation_problem_details_with_400()
    {
        object request = new
        {
            sku = "bad sku",
            name = "x",
            category = "Premium",
            price = 10m,
            metadata = new { color = "", stock = -1 }
        };

        using HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Request validation failed.", body.GetProperty("title").GetString());
        Assert.True(body.TryGetProperty("errors", out JsonElement errors));
        Assert.True(errors.EnumerateObject().Any());
    }

    [Fact]
    public async Task Duplicate_sku_returns_conflict_problem_details_with_409()
    {
        object request = new
        {
            sku = "CSHARP-101",
            name = "Duplicate",
            category = "Course",
            price = 50m,
            metadata = new { color = "Red", stock = 1 }
        };

        using HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Conflict", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Missing_product_returns_not_found_problem_details_with_404()
    {
        using HttpResponseMessage response = await _client.GetAsync("/api/products/999");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Product not found", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Domain_rule_returns_unprocessable_entity_with_422()
    {
        using HttpResponseMessage response = await _client.DeleteAsync("/api/products/1");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Business rule failed", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Form_binding_reads_url_encoded_form_data()
    {
        using FormUrlEncodedContent form = new(new Dictionary<string, string>
        {
            ["Note"] = "  submitted from a form  "
        });

        using HttpResponseMessage response = await _client.PostAsync("/api/products/form-note", form);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("submitted from a form", body.GetProperty("received").GetString());
        Assert.Equal("form", body.GetProperty("source").GetString());
    }

    [Fact]
    public async Task Unexpected_exception_returns_generic_problem_details_with_500()
    {
        using HttpResponseMessage response = await _client.GetAsync("/api/products/failure-demo");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("An unexpected error occurred.", body.GetProperty("title").GetString());

        // The server may omit a null detail or serialize it as JSON null, but
        // it must never expose the original exception message to the client.
        if (body.TryGetProperty("detail", out JsonElement detail))
        {
            Assert.Equal(JsonValueKind.Null, detail.ValueKind);
        }
    }
}
