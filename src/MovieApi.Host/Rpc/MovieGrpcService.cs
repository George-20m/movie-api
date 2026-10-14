using Grpc.Core;
using MovieApi.Core;

namespace MovieApi.Host.Rpc;

public class MovieGrpcService : MovieLookup.MovieLookupBase
{
    private readonly MovieCatalog _catalog;

    public MovieGrpcService(MovieCatalog catalog) => _catalog = catalog;

    public override async Task<MovieReply> GetMovie(GetMovieRequest request, ServerCallContext context)
    {
        var movie = await _catalog.GetAsync(request.Id, context.CancellationToken);
        if (movie is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Movie {request.Id} not found"));
        return ToReply(movie);
    }

    public override async Task<SearchMoviesReply> SearchMovies(SearchMoviesRequest request, ServerCallContext context)
    {
        var movies = await _catalog.SearchAsync(request.TitlePrefix, request.Limit, context.CancellationToken);
        var reply = new SearchMoviesReply();
        reply.Movies.AddRange(movies.Select(ToReply));
        return reply;
    }

    private static MovieReply ToReply(Movie m) => new()
    {
        Id = m.Id,
        Title = m.Title,
        Overview = m.Overview ?? "",
        ReleaseDate = m.ReleaseDate?.ToString("yyyy-MM-dd") ?? "",
        Popularity = m.Popularity ?? 0,
        VoteAverage = m.VoteAverage ?? 0,
        VoteCount = m.VoteCount ?? 0
    };
}
