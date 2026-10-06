using System.Net;

namespace HackerNewsBestStories.FunctionalTests;

public class BestStoriesApiTests
{
    [Fact]
    public async Task GetBest_ReturnsTheHighestScoredStoriesInTheRequiredFormat()
    {
        await using var api = new ApiFactory();
        api.HackerNews.BestStories(1, 2, 3);
        api.HackerNews.Story(1, score: 50);
        api.HackerNews.Story(2, score: 300);
        api.HackerNews.Story(3, score: 120);
        var client = await api.CreateClientAfterFirstRefreshAsync();

        var json = await client.GetStringAsync("/api/stories/best?count=1");

        Assert.Equal(
            """[{"title":"Story 2","uri":"https://example.com","postedBy":"user2","time":"2019-10-12T13:43:01+00:00","score":300,"commentCount":7}]""",
            json);
    }

    [Fact]
    public async Task GetBest_ManyRequests_NeverReachHackerNews()
    {
        await using var api = new ApiFactory();
        api.HackerNews.BestStories(1);
        api.HackerNews.Story(1, score: 10);
        var client = await api.CreateClientAfterFirstRefreshAsync();
        var requestsAfterFirstRefresh = api.HackerNews.Requests;

        for (var i = 0; i < 50; i++)
        {
            var response = await client.GetAsync("/api/stories/best?count=10");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        Assert.Equal(requestsAfterFirstRefresh, api.HackerNews.Requests);
    }

    [Fact]
    public async Task GetBest_BeforeTheFirstSnapshot_Returns503()
    {
        await using var api = new ApiFactory();
        var client = api.CreateClient();

        var response = await client.GetAsync("/api/stories/best?count=10");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?count=0")]
    [InlineData("?count=abc")]
    public async Task GetBest_WithInvalidCount_Returns400(string query)
    {
        await using var api = new ApiFactory();
        var client = api.CreateClient();

        var response = await client.GetAsync("/api/stories/best" + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
