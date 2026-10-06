# Hacker News Best Stories

ASP.NET Core API (.NET 10) that returns the first `n` best stories from the [Hacker News API](https://github.com/HackerNews/API), ordered by score.

```
GET /api/stories/best?count=10
```

## Run

```bash
dotnet run --project src/HackerNewsBestStories --launch-profile http
```

Swagger: http://localhost:5029/swagger

Response:

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

### Docker

```bash
docker build -t hackernews-best-stories .
docker run --rm -p 8080:8080 hackernews-best-stories
```

The container runs as Production so Swagger is disabled. Add `-e ASPNETCORE_ENVIRONMENT=Development` if you want it.

Settings can be overridden with env vars, e.g. `-e BestStories__RefreshInterval=00:05:00`.

## Tests

```bash
dotnet test
```

None of the tests call the real Hacker News API.

## Config

```json
"HackerNews": {
  "BaseUrl": "https://hacker-news.firebaseio.com/v0/"
},
"BestStories": {
  "RefreshInterval": "00:02:00",
  "MaxConcurrentRequests": 10
}
```

Both sections are validated on startup.

## Assumptions

- Data can be up to 2 minutes old (`RefreshInterval`). For a best stories list I think that's fine.
- `count` is required and must be >= 1, otherwise it returns 400. If it's bigger than the number of stories available (HN returns around 200 ids) it just returns all of them.
- Jobs, deleted/dead items and ids that return `null` are skipped.
- Right after startup, until the first refresh finishes (a couple of seconds), the API returns 503.

## With more time

- Move the store to Redis and run the refresher as a separate worker, so the API can run on several instances.
- Use `/v0/updates` to only fetch the items that changed instead of all of them on every refresh.
