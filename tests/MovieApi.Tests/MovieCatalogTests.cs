using MovieApi.Core;
using Xunit;

namespace MovieApi.Tests;

public class MovieCatalogTests
{
    [Fact]
    public async Task Get_NonPositiveId_ReturnsNull()
    {
        var catalog = new MovieCatalog(new FakeRepo());
        Assert.Null(await catalog.GetAsync(0));
    }

    [Fact]
    public async Task Search_BlankPrefix_ReturnsEmpty_WithoutQueryingRepo()
    {
        var repo = new FakeRepo();
        var result = await new MovieCatalog(repo).SearchAsync("   ", 5);
        Assert.Empty(result);
        Assert.Null(repo.LastPrefix);
    }

    [Theory]
    [InlineData(null, 20)]
    [InlineData(0, 20)]
    [InlineData(5, 5)]
    [InlineData(1000, 100)]
    public async Task Search_AppliesLimitRules(int? input, int expected)
    {
        var repo = new FakeRepo();
        await new MovieCatalog(repo).SearchAsync("a", input);
        Assert.Equal(expected, repo.LastLimit);
    }

    [Fact]
    public async Task Search_TrimsPrefix()
    {
        var repo = new FakeRepo();
        await new MovieCatalog(repo).SearchAsync("  mor ", 5);
        Assert.Equal("mor", repo.LastPrefix);
    }
}
