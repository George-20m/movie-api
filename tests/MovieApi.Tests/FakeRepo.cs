using MovieApi.Core;

namespace MovieApi.Tests;

public sealed class FakeRepo : IMovieRepository
{
    public string? LastPrefix;
    public int LastLimit;
    public List<Movie> Movies { get; } = new();

    public Task<Movie?> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Movies.FirstOrDefault(m => m.Id == id));

    public Task<IReadOnlyList<Movie>> SearchByTitlePrefixAsync(string prefix, int limit, CancellationToken ct = default)
    {
        LastPrefix = prefix;
        LastLimit = limit;
        IReadOnlyList<Movie> result = Movies
            .Where(m => m.Title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<bool> PingAsync(CancellationToken ct = default) => Task.FromResult(true);
}
