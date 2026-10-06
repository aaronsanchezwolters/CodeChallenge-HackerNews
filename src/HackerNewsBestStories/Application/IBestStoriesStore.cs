using HackerNewsBestStories.Domain;

namespace HackerNewsBestStories.Application;

public interface IBestStoriesStore
{
    Task<IReadOnlyList<Story>> GetBestAsync(int count, CancellationToken cancellationToken);

    Task ReplaceAsync(IReadOnlyList<Story> stories, CancellationToken cancellationToken);
}
