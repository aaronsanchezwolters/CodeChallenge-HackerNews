using HackerNewsBestStories.Application;
using HackerNewsBestStories.Domain;

namespace HackerNewsBestStories.Infrastructure;

public sealed class InMemoryBestStoriesStore : IBestStoriesStore
{
    private volatile IReadOnlyList<Story> _stories = [];

    public Task<IReadOnlyList<Story>> GetBestAsync(int count, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Story>>(_stories.Take(count).ToArray());

    public Task ReplaceAsync(IReadOnlyList<Story> stories, CancellationToken cancellationToken)
    {
        _stories = stories;
        return Task.CompletedTask;
    }
}
