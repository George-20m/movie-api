namespace MovieApi.Core;

public sealed record Movie(
    long Id,
    string Title,
    string? Overview,
    DateTime? ReleaseDate,
    double? Popularity,
    double? VoteAverage,
    long? VoteCount);
