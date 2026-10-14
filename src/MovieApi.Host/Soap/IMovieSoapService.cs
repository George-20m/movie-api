using System.Runtime.Serialization;
using System.ServiceModel;

namespace MovieApi.Host.Soap;

[ServiceContract(Namespace = "http://movieapi.example/soap")]
public interface IMovieSoapService
{
    [OperationContract]
    Task<MovieSoap?> GetMovie(long id);

    [OperationContract]
    Task<List<MovieSoap>> SearchMovies(string titlePrefix, int limit);
}

[DataContract(Namespace = "http://movieapi.example/soap")]
public class MovieSoap
{
    [DataMember(Order = 1)] public long Id { get; set; }
    [DataMember(Order = 2)] public string Title { get; set; } = "";
    [DataMember(Order = 3)] public string? Overview { get; set; }
    [DataMember(Order = 4)] public DateTime? ReleaseDate { get; set; }
    [DataMember(Order = 5)] public double? Popularity { get; set; }
    [DataMember(Order = 6)] public double? VoteAverage { get; set; }
    [DataMember(Order = 7)] public long? VoteCount { get; set; }
}
