namespace MovieApi.Core;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<Movie>> SearchByTitlePrefixAsync(string prefix, int limit, CancellationToken ct = default);
    Task<bool> PingAsync(CancellationToken ct = default);
}
