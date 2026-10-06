FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/HackerNewsBestStories/HackerNewsBestStories.csproj src/HackerNewsBestStories/
RUN dotnet restore src/HackerNewsBestStories/HackerNewsBestStories.csproj

COPY src/ src/
RUN dotnet publish src/HackerNewsBestStories/HackerNewsBestStories.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "HackerNewsBestStories.dll"]
