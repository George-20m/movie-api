using MovieApi.Core;

namespace MovieApi.Host.Soap;

public class MovieSoapService : IMovieSoapService
{
    private readonly MovieCatalog _catalog;

    public MovieSoapService(MovieCatalog catalog) => _catalog = catalog;

    public async Task<MovieSoap?> GetMovie(long id)
    {
        var movie = await _catalog.GetAsync(id);
        return movie is null ? null : ToSoap(movie);
    }

    public async Task<List<MovieSoap>> SearchMovies(string titlePrefix, int limit)
    {
        var movies = await _catalog.SearchAsync(titlePrefix, limit);
        return movies.Select(ToSoap).ToList();
    }

    private static MovieSoap ToSoap(Movie m) => new()
    {
        Id = m.Id,
        Title = m.Title,
        Overview = m.Overview,
        ReleaseDate = m.ReleaseDate,
        Popularity = m.Popularity,
        VoteAverage = m.VoteAverage,
        VoteCount = m.VoteCount
    };
}
