using HackerNewsBestStories.Application;
using HackerNewsBestStories.Domain;

namespace HackerNewsBestStories.Infrastructure;

public sealed class HackerNewsClient(IHttpClientFactory httpClientFactory) : IHackerNewsClient
{
    public const string Name = "HackerNews";

    public async Task<int[]> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        return await httpClientFactory.CreateClient(Name).GetFromJsonAsync<int[]>("beststories.json", cancellationToken) ?? [];
    }

    public async Task<Story?> GetStoryAsync(int id, CancellationToken cancellationToken)
    {
        var item = await httpClientFactory.CreateClient(Name).GetFromJsonAsync<HackerNewsItem>($"item/{id}.json", cancellationToken);
        return item?.ToStory();
    }
}
