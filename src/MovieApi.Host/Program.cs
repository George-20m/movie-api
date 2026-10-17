using MovieApi.Core;
using MovieApi.Data;
using MovieApi.Host.Rpc;
using MovieApi.Host.Soap;
using SoapCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMovieRepository>(sp =>
    new OracleMovieRepository(
        sp.GetRequiredService<IConfiguration>().GetConnectionString("Oracle")
        ?? throw new InvalidOperationException("ConnectionStrings:Oracle is not set.")));
builder.Services.AddSingleton<MovieCatalog>();
builder.Services.AddScoped<IMovieSoapService, MovieSoapService>();
builder.Services.AddSoapCore();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

((IEndpointRouteBuilder)app).UseSoapEndpoint<IMovieSoapService>(opt =>
{
    opt.Path = "/soap/movies";
    opt.SoapSerializer = SoapSerializer.DataContractSerializer;
});

app.MapGrpcService<MovieGrpcService>();
app.MapGrpcReflectionService();

app.MapGet("/health", async (MovieCatalog catalog, CancellationToken ct) =>
    await catalog.IsDatabaseUpAsync(ct)
        ? Results.Ok(new { status = "ok" })
        : Results.StatusCode(503))
    .WithTags("Health");

app.MapGet("/api/movies/{id:long}", async (long id, MovieCatalog catalog, CancellationToken ct) =>
    await catalog.GetAsync(id, ct) is { } movie ? Results.Ok(movie) : Results.NotFound())
    .WithTags("Movies");

app.MapGet("/api/movies", async (string? title, int? limit, MovieCatalog catalog, CancellationToken ct) =>
    Results.Ok(await catalog.SearchAsync(title, limit, ct)))
    .WithTags("Movies");

app.MapGet("/api/version", () => Results.Ok(new { version = "1.0.1" })).WithTags("Health");

app.Run();

public partial class Program { }
