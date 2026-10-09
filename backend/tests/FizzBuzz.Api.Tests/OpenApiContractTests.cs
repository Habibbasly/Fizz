using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FizzBuzz.Api.Tests;

/// <summary>
/// Vérifie le contrat consommé par le front Angular : un changement ici est un changement cassant.
/// </summary>
public class OpenApiContractTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private async Task<JsonElement> GetFizzBuzzOperationAsync()
    {
        var document = JsonDocument.Parse(await factory.CreateClient().GetStringAsync("/openapi/v1.json"));
        return document.RootElement.GetProperty("paths").GetProperty("/api/FizzBuzz").GetProperty("get");
    }

    [Fact]
    public async Task FizzBuzzEndpoint_ExposesRequiredQueryParameters()
    {
        var operation = await GetFizzBuzzOperationAsync();

        var parameters = operation.GetProperty("parameters").EnumerateArray()
            .ToDictionary(p => p.GetProperty("name").GetString()!);

        Assert.Equal(["int1", "int2", "limit", "str1", "str2"], parameters.Keys.Order());
        Assert.All(parameters.Values, p =>
        {
            Assert.Equal("query", p.GetProperty("in").GetString());
            Assert.True(p.GetProperty("required").GetBoolean());
        });

        Assert.All(["int1", "int2", "limit"], name => Assert.Contains("integer", SchemaTypes(parameters[name])));
        Assert.All(["str1", "str2"], name => Assert.Equal(["string"], SchemaTypes(parameters[name])));
    }

    // OpenAPI 3.1 : "type" peut être une chaîne ou un tableau (ex : ["integer", "string"] pour un entier en query).
    private static string[] SchemaTypes(JsonElement parameter)
    {
        var type = parameter.GetProperty("schema").GetProperty("type");
        return type.ValueKind == JsonValueKind.Array
            ? type.EnumerateArray().Select(t => t.GetString()!).ToArray()
            : [type.GetString()!];
    }

    [Fact]
    public async Task FizzBuzzEndpoint_DocumentsOkAndBadRequest()
    {
        var operation = await GetFizzBuzzOperationAsync();
        var responses = operation.GetProperty("responses");

        Assert.True(responses.TryGetProperty("200", out var ok));
        Assert.True(responses.TryGetProperty("400", out _));

        var schema = ok.GetProperty("content").GetProperty("application/json").GetProperty("schema");
        Assert.Equal("array", schema.GetProperty("type").GetString());
        Assert.Equal("string", schema.GetProperty("items").GetProperty("type").GetString());
    }
}
