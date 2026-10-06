using HackerNewsBestStories.Background;
using HackerNewsBestStories.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace HackerNewsBestStories.UnitTests;

public class BestStoriesRefresherTests
{
    private readonly FakeHackerNewsClient _hackerNews = new();
    private readonly InMemoryBestStoriesStore _store = new();
    private readonly FakeTimeProvider _time = new();
    private readonly BestStoriesOptions _options = new();
    private readonly BestStoriesRefresher _refresher;

    public BestStoriesRefresherTests()
    {
        _refresher = new BestStoriesRefresher(
            _hackerNews,
            _store,
            Options.Create(_options),
            _time,
            NullLogger<BestStoriesRefresher>.Instance);
    }

    [Fact]
    public async Task Refresh_SortsByScoreAndSkipsItemsThatAreNotStories()
    {
        _hackerNews.BestStories(1, 2, 3, 4);
        _hackerNews.Story(1, score: 50);
        _hackerNews.Story(2, score: 300);
        _hackerNews.NotAStory(3);
        _hackerNews.Story(4, score: 120);

        await _refresher.Refresh(CancellationToken.None);

        var scores = await StoredScoresAsync();
        Assert.Equal([300, 120, 50], scores);
    }

    [Fact]
    public async Task Refresh_WhenBestStoriesFail_KeepsThePreviousSnapshot()
    {
        _hackerNews.BestStories(1);
        _hackerNews.Story(1, score: 10);
        await _refresher.Refresh(CancellationToken.None);

        _hackerNews.BestStoriesFail();
        await _refresher.Refresh(CancellationToken.None);

        var scores = await StoredScoresAsync();
        Assert.Equal([10], scores);
    }

    [Fact]
    public async Task Refresh_WhenAStoryFails_KeepsItsLastKnownVersionOrSkipsItIfNew()
    {
        _hackerNews.BestStories(1, 2);
        _hackerNews.Story(1, score: 300);
        _hackerNews.Story(2, score: 200);
        await _refresher.Refresh(CancellationToken.None);

        _hackerNews.BestStories(1, 2, 3);
        _hackerNews.StoryFails(1);
        _hackerNews.Story(2, score: 250);
        _hackerNews.StoryFails(3);
        await _refresher.Refresh(CancellationToken.None);

        var scores = await StoredScoresAsync();
        Assert.Equal([300, 250], scores);
    }

    [Fact]
    public async Task Refresh_WhenAKnownStoryIsNoLongerAStory_DropsIt()
    {
        _hackerNews.BestStories(1, 2);
        _hackerNews.Story(1, score: 300);
        _hackerNews.Story(2, score: 200);
        await _refresher.Refresh(CancellationToken.None);

        _hackerNews.NotAStory(1);
        await _refresher.Refresh(CancellationToken.None);

        var scores = await StoredScoresAsync();
        Assert.Equal([200], scores);
    }

    [Fact]
    public async Task Execute_RefreshesAgainAfterTheRefreshInterval()
    {
        _hackerNews.BestStories(1);
        _hackerNews.Story(1, score: 10);
        await _refresher.StartAsync(CancellationToken.None);
        await WaitForStoredScoresAsync([10]);

        _hackerNews.Story(1, score: 15);
        _time.Advance(_options.RefreshInterval);

        await WaitForStoredScoresAsync([15]);
        await _refresher.StopAsync(CancellationToken.None);
    }

    private async Task<int[]> StoredScoresAsync()
    {
        var stories = await _store.GetBestAsync(int.MaxValue, CancellationToken.None);
        return stories.Select(story => story.Score).ToArray();
    }

    private async Task WaitForStoredScoresAsync(int[] expected)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!(await StoredScoresAsync()).SequenceEqual(expected))
        {
            await Task.Delay(10, timeout.Token);
        }
    }
}
