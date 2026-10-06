using HackerNewsBestStories.Application;
using HackerNewsBestStories.Domain;
using Microsoft.Extensions.Options;

namespace HackerNewsBestStories.Background;

public sealed class BestStoriesRefresher(
    IHackerNewsClient client,
    IBestStoriesStore store,
    IOptions<BestStoriesOptions> options,
    TimeProvider timeProvider,
    ILogger<BestStoriesRefresher> logger) : BackgroundService
{
    private readonly BestStoriesOptions _options = options.Value;

    private IReadOnlyDictionary<int, Story> _lastKnownStories = new Dictionary<int, Story>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.RefreshInterval, timeProvider);
        do
        {
            await Refresh(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task Refresh(CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetTimestamp();
        try
        {
            var ids = await client.GetBestStoryIdsAsync(cancellationToken);
            var results = await GetStories(ids, cancellationToken);
            _lastKnownStories = results
                .Where(result => result.Story is not null)
                .DistinctBy(result => result.Id)
                .ToDictionary(result => result.Id, result => result.Story!);

            var stories = SortByScore(results.Select(result => result.Story));
            if (await SaveSnapshot(stories, cancellationToken))
            {
                LogRefreshed(stories.Length, results, timeProvider.GetElapsedTime(startedAt));
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Refreshing best stories failed; keeping the previous snapshot");
        }
    }

    private async Task<FetchResult[]> GetStories(int[] ids, CancellationToken cancellationToken)
    {
        var results = new FetchResult[ids.Length];
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxConcurrentRequests,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(ids.Index(), parallelOptions, async (entry, cancellationToken) =>
        {
            var (index, id) = entry;
            results[index] = await GetStory(id, cancellationToken);
        });

        return results;
    }

    /// <summary>Return HackerNews Story by id if fails trys to use allready cached Story.</summary>
    private async Task<FetchResult> GetStory(int id, CancellationToken cancellationToken)
    {
        try
        {
            return new FetchResult(id, await client.GetStoryAsync(id, cancellationToken), Failed: false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogDebug(ex, "Could not fetch item {Id}", id);
            return new FetchResult(id, _lastKnownStories.GetValueOrDefault(id), Failed: true);
        }
    }

    private static Story[] SortByScore(IEnumerable<Story?> stories)
    {
        return stories.OfType<Story>()
            .OrderByDescending(story => story.Score)
            .ToArray();
    }

    private async Task<bool> SaveSnapshot(Story[] stories, CancellationToken cancellationToken)
    {
        if (stories.Length == 0)
        {
            logger.LogWarning("Refresh produced no stories; keeping the previous snapshot");
            return false;
        }

        await store.ReplaceAsync(stories, cancellationToken);
        return true;
    }

    private void LogRefreshed(int count, FetchResult[] results, TimeSpan elapsed)
    {
        var failed = results.Count(result => result.Failed);
        if (failed == 0)
        {
            logger.LogInformation("Refreshed best stories in {ElapsedMs:F0} ms: {Count} stories", elapsed.TotalMilliseconds, count);
            return;
        }

        var reused = results.Count(result => result.Failed && result.Story is not null);
        logger.LogWarning(
            "Refreshed best stories in {ElapsedMs:F0} ms: {Count} stories, but {Failed} items could not be fetched ({Reused} kept their previous version)",
            elapsed.TotalMilliseconds, count, failed, reused);
    }

    private readonly record struct FetchResult(int Id, Story? Story, bool Failed);
}
