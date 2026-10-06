using HackerNewsBestStories.Domain;

namespace HackerNewsBestStories.Application;

public interface IHackerNewsClient
{
    Task<int[]> GetBestStoryIdsAsync(CancellationToken cancellationToken);

    Task<Story?> GetStoryAsync(int id, CancellationToken cancellationToken);
}
