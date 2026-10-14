namespace MovieApi.Core;

public sealed class MovieCatalog
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 100;

    private readonly IMovieRepository _repo;

    public MovieCatalog(IMovieRepository repo) => _repo = repo;

    public Task<Movie?> GetAsync(long id, CancellationToken ct = default) =>
        id <= 0 ? Task.FromResult<Movie?>(null) : _repo.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<Movie>> SearchAsync(string? prefix, int? limit, CancellationToken ct = default)
    {
        var p = prefix?.Trim();
        if (string.IsNullOrEmpty(p))
            return Array.Empty<Movie>();

        var n = limit is > 0 ? Math.Min(limit.Value, MaxLimit) : DefaultLimit;
        return await _repo.SearchByTitlePrefixAsync(p, n, ct);
    }

    public Task<bool> IsDatabaseUpAsync(CancellationToken ct = default) => _repo.PingAsync(ct);
}
