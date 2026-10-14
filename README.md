# Oracle-Backed Multi-Protocol API Platform

An API that serves a 344K-row movie dataset from **Oracle Database** over three protocols at once:
**REST**, **SOAP** and **gRPC**. All three call the same core layer, so the only thing that differs
between them is how a request travels.

## Architecture

```
            REST (HTTP/1.1, :5000)   SOAP (HTTP/1.1, :5000)   gRPC (HTTP/2, :5001)
                        \                    |                    /
                         +----------- MovieApi.Host -------------+
                                           |
                                    MovieApi.Core        (MovieCatalog: validation, limits)
                                           |
                                    MovieApi.Data        (OracleMovieRepository: bind variables)
                                           |
                                    Oracle Database Free (Docker, schema MOVIES_APP)
```

## Quick start

Requirements: Docker with the Compose plugin.

```bash
cp .env.example .env        # then edit the two passwords (long, random)
docker volume create oracle-data
docker compose up -d --build
docker compose ps           # wait until oracle is "healthy"
```

On a brand-new database you must also create the table and load the data (see "Data").

Local development without Docker for the API:

```bash
source scripts/dev-env.sh   # exports the connection string (reads ~/oracle.env)
dotnet run --project src/MovieApi.Host
```

## Using the API

REST (Swagger UI at http://localhost:5000/swagger):

```bash
curl http://localhost:5000/api/movies/526896
curl "http://localhost:5000/api/movies?title=morb&limit=5"
curl http://localhost:5000/health
```

gRPC (reflection is enabled; use [grpcurl](https://github.com/fullstorydev/grpcurl)):

```bash
grpcurl -plaintext -d '{"id": 526896}' localhost:5001 movies.v1.MovieLookup/GetMovie
grpcurl -plaintext -d '{"title_prefix": "morb", "limit": 5}' localhost:5001 movies.v1.MovieLookup/SearchMovies
```

SOAP (WSDL at http://localhost:5000/soap/movies?wsdl):

```bash
curl -X POST http://localhost:5000/soap/movies \
  -H 'Content-Type: text/xml; charset=utf-8' \
  -H 'SOAPAction: "http://movieapi.example/soap/IMovieSoapService/GetMovie"' \
  -d '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><GetMovie xmlns="http://movieapi.example/soap"><id>526896</id></GetMovie></s:Body></s:Envelope>'
```

## Data

Source: a Kaggle movies CSV (433,827 rows, 373 MB). It is not stored in this repository.

Loading rules (implemented in `scripts/load_movies.py`):

- Repeated ids: the first occurrence in the file is kept; later copies are skipped (89,752 rows).
  Copies differ only in fast-changing numbers such as popularity and vote counts.
- Rows with an empty title are skipped (1 row, an empty placeholder).
- Result: 344,074 movies loaded in about 15 seconds using batches of 5,000 rows and one commit per batch.
- Only the core `movies` table is loaded. Genres, cast, keywords and recommendations are out of scope.

```bash
python3 -m venv .venv && source .venv/bin/activate && pip install -r scripts/requirements.txt
export DB_PASSWORD=...        # the application user's password
python3 scripts/run_load.py /path/to/movies.csv
```

## Indexing

Title search is case-insensitive prefix search, backed by a function-based index
(`sql/01_indexes.sql`). Measured on 344,074 rows:

| Query | Plan | Result |
|---|---|---|
| 200 lookups by title, no index | TABLE ACCESS FULL, cost 4950 | 8.7 s |
| 200 lookups by title, index on `title` | INDEX RANGE SCAN, cost 5 | 0.018 s (about 480x faster) |
| `UPPER(title) = ...` with the plain index | Both indexes scanned completely, cost 3581 | cannot seek |
| `UPPER(title) LIKE 'MORB%'`, index on `UPPER(title)` | INDEX RANGE SCAN, cost 5 | what the API uses |

Wrapping a column in a function stops a plain index from being used for a seek, so the API's index is built on
`UPPER(title)`. Cost on disk: the index is 13 MB next to a 144 MB table.

## Security

- The application connects as a least-privilege user, not SYS or SYSTEM.
- All SQL uses bind variables; LIKE wildcards in user input are escaped.
- Secrets live in `.env` (git-ignored) and are injected at runtime; the Oracle and API ports are published
  on 127.0.0.1 only.
- gRPC runs without TLS for local development. Put a TLS-terminating proxy in front for real deployments.

## Tests

```bash
dotnet test
```

Unit and REST tests run without a database. Oracle integration tests run only when `ConnectionStrings__Oracle`
is set (for example after `source scripts/dev-env.sh`), and are skipped otherwise.

## CI/CD

`Jenkinsfile` runs the tests in a .NET SDK container, deploys with Docker Compose and smoke-tests `/health`.
Jenkins polls this repository and receives the `.env` file as a secret-file credential.

## Project layout

```
src/MovieApi.Core   model, repository contract, MovieCatalog
src/MovieApi.Data   Oracle repository
src/MovieApi.Host   REST, SOAP, gRPC endpoints
tests/              unit, REST and Oracle integration tests
scripts/            Python data loader and dev environment script
sql/                index definitions
```

## Limitations

- Swagger documents the REST endpoints only.
- Only two operations are exposed (get by id, search by title prefix).
- Single Oracle instance; no replication or backups are configured.
