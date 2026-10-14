using MovieApi.Data;
using Xunit;

namespace MovieApi.Tests;

// Skipped automatically when no database connection string is set (for example in CI).
public sealed class OracleFactAttribute : FactAttribute
{
    public OracleFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("ConnectionStrings__Oracle") is null)
            Skip = "Set ConnectionStrings__Oracle to run the Oracle integration tests.";
    }
}

public class OracleRepositoryTests
{
    private static OracleMovieRepository Repo() =>
        new(Environment.GetEnvironmentVariable("ConnectionStrings__Oracle")!);

    [OracleFact]
    public async Task GetById_ReturnsMorbius()
    {
        var movie = await Repo().GetByIdAsync(526896);
        Assert.NotNull(movie);
        Assert.Equal("Morbius", movie!.Title);
    }

    [OracleFact]
    public async Task Search_IsCaseInsensitivePrefix()
    {
        var list = await Repo().SearchByTitlePrefixAsync("morb", 10);
        Assert.Contains(list, m => m.Id == 526896);
    }
}
