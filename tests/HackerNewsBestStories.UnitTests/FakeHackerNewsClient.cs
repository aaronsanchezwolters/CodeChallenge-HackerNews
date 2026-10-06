using HackerNewsBestStories.Application;
using HackerNewsBestStories.Domain;

namespace HackerNewsBestStories.UnitTests;

public sealed class FakeHackerNewsClient : IHackerNewsClient
{
    private readonly Dictionary<int, Story?> _stories = [];
    private int[]? _bestStoryIds = [];

    public void BestStories(params int[] ids)
    {
        _bestStoryIds = ids;
    }

    public void BestStoriesFail()
    {
        _bestStoryIds = null;
    }

    public void Story(int id, int score)
    {
        _stories[id] = new Story($"Story {id}", null, $"user{id}", DateTimeOffset.UnixEpoch, score, 0);
    }

    public void NotAStory(int id)
    {
        _stories[id] = null;
    }

    public void StoryFails(int id)
    {
        _stories.Remove(id);
    }

    public Task<int[]> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        return _bestStoryIds is null
            ? Task.FromException<int[]>(new HttpRequestException("beststories.json failed"))
            : Task.FromResult(_bestStoryIds);
    }

    public Task<Story?> GetStoryAsync(int id, CancellationToken cancellationToken)
    {
        return _stories.TryGetValue(id, out var story)
            ? Task.FromResult(story)
            : Task.FromException<Story?>(new HttpRequestException($"item/{id}.json failed"));
    }
}
