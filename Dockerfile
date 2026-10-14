FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ src/
RUN dotnet restore src/MovieApi.Host/MovieApi.Host.csproj
RUN dotnet publish src/MovieApi.Host/MovieApi.Host.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "MovieApi.Host.dll"]
