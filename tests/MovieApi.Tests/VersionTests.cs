using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MovieApi.Tests;

public class VersionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public VersionTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Version_ReturnsVersionNumber()
    {
        var body = await _client.GetStringAsync("/api/version");
        Assert.Contains("1.0.1", body);
    }
}
