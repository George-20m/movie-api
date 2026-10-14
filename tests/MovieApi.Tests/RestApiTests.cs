using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MovieApi.Core;
using Xunit;

namespace MovieApi.Tests;

public class RestApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RestApiTests(WebApplicationFactory<Program> factory)
    {
        var repo = new FakeRepo();
        repo.Movies.Add(new Movie(1, "Morbius", "A doctor.", new DateTime(2022, 3, 30), 3124.0, 6.4, 1670));
        _client = factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.RemoveAll<IMovieRepository>();
            s.AddSingleton<IMovieRepository>(repo);
        })).CreateClient();
    }

    [Fact]
    public async Task GetExisting_Returns200WithTitle()
    {
        var response = await _client.GetAsync("/api/movies/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Morbius", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetMissing_Returns404()
    {
        var response = await _client.GetAsync("/api/movies/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_ReturnsMatch()
    {
        var body = await _client.GetStringAsync("/api/movies?title=mor");
        Assert.Contains("Morbius", body);
    }
}
