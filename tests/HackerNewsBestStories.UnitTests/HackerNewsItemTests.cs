using HackerNewsBestStories.Domain;
using HackerNewsBestStories.Infrastructure;

namespace HackerNewsBestStories.UnitTests;

public class HackerNewsItemTests
{
    [Fact]
    public void ToStory_MapsTheItemToAStory()
    {
        var item = new HackerNewsItem(Id: 1, Type: "story", By: "pg", Time: 1570887781, Title: "Ask HN",
                                      Url: null, Score: 5, Descendants: null, Deleted: false, Dead: false);

        var story = item.ToStory();

        var expected = new Story(Title: "Ask HN",
                                 Uri: null,
                                 PostedBy: "pg",
                                 Time: new DateTimeOffset(2019, 10, 12, 13, 43, 1, TimeSpan.Zero),
                                 Score: 5,
                                 CommentCount: 0);
        Assert.Equal(expected, story);
    }

    [Theory]
    [InlineData("job", false, false)]
    [InlineData("story", true, false)]
    [InlineData("story", false, true)]
    public void ToStory_WhenItemIsNotALiveStory_ReturnsNull(string type, bool deleted, bool dead)
    {
        var item = new HackerNewsItem(Id: 1, Type: type, By: "pg", Time: 1570887781, Title: "Title",
                                      Url: "https://example.com", Score: 999, Descendants: 3, Deleted: deleted, Dead: dead);

        Assert.Null(item.ToStory());
    }
}
