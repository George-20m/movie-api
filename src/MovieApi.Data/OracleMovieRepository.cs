using System.Data.Common;
using MovieApi.Core;
using Oracle.ManagedDataAccess.Client;

namespace MovieApi.Data;

public sealed class OracleMovieRepository : IMovieRepository
{
    private const string Columns =
        "id, title, overview, release_date, popularity, vote_average, vote_count";

    private readonly string _connectionString;

    public OracleMovieRepository(string connectionString) => _connectionString = connectionString;

    public async Task<Movie?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.BindByName = true;
        cmd.CommandText = $"SELECT {Columns} FROM movies WHERE id = :id";
        cmd.Parameters.Add("id", OracleDbType.Int64).Value = id;
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<Movie>> SearchByTitlePrefixAsync(string prefix, int limit, CancellationToken ct = default)
    {
        // Escape LIKE wildcards so a user typing % or _ searches for those characters literally.
        var escaped = prefix.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.BindByName = true;
        cmd.CommandText =
            $"SELECT {Columns} FROM movies " +
            "WHERE UPPER(title) LIKE UPPER(:prefix) || '%' ESCAPE '\\' " +
            "ORDER BY title FETCH FIRST :lim ROWS ONLY";
        cmd.Parameters.Add("prefix", OracleDbType.Varchar2).Value = escaped;
        cmd.Parameters.Add("lim", OracleDbType.Int32).Value = limit;

        var result = new List<Movie>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            result.Add(Map(reader));
        return result;
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try
        {
            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM dual";
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) == 1;
        }
        catch (OracleException)
        {
            return false;
        }
    }

    private static Movie Map(DbDataReader r) => new(
        Convert.ToInt64(r.GetValue(0)),
        r.GetString(1),
        r.IsDBNull(2) ? null : r.GetString(2),
        r.IsDBNull(3) ? null : r.GetDateTime(3),
        r.IsDBNull(4) ? null : Convert.ToDouble(r.GetValue(4)),
        r.IsDBNull(5) ? null : Convert.ToDouble(r.GetValue(5)),
        r.IsDBNull(6) ? null : Convert.ToInt64(r.GetValue(6)));
}
