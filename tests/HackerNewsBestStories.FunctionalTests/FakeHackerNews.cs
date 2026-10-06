using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace HackerNewsBestStories.FunctionalTests;

public sealed class FakeHackerNews : HttpMessageHandler
{
    public static readonly Uri BaseAddress = new("https://hn.test/v0/");

    private readonly ConcurrentDictionary<string, string> _responses = new();
    private int _requests;

    public int Requests => Volatile.Read(ref _requests);

    public void BestStories(params int[] ids)
    {
        _responses["beststories.json"] = JsonSerializer.Serialize(ids);
    }

    public void Story(int id, int score)
    {
        _responses[$"item/{id}.json"] = JsonSerializer.Serialize(new
        {
            id,
            type = "story",
            by = $"user{id}",
            time = 1570887781,
            title = $"Story {id}",
            url = "https://example.com",
            score,
            descendants = 7,
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _requests);

        var path = request.RequestUri!.AbsolutePath[BaseAddress.AbsolutePath.Length..];
        var response = _responses.TryGetValue(path, out var body)
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) }
            : new HttpResponseMessage(HttpStatusCode.InternalServerError);

        return Task.FromResult(response);
    }
}
