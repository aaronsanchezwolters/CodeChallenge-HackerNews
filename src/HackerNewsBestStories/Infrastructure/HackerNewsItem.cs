using HackerNewsBestStories.Domain;

namespace HackerNewsBestStories.Infrastructure;

public sealed record HackerNewsItem(
    int Id,
    string? Type,
    string? By,
    long Time,
    string? Title,
    string? Url,
    int Score,
    int? Descendants,
    bool Deleted,
    bool Dead)
{
    public Story? ToStory()
    {
        if (Type is not "story" || Deleted || Dead)
        {
            return null;
        }

        return new Story(Title: Title ?? string.Empty,
                         Uri: string.IsNullOrWhiteSpace(Url) ? null : Url,
                         PostedBy: By ?? string.Empty,
                         Time: DateTimeOffset.FromUnixTimeSeconds(Time),
                         Score: Score,
                         CommentCount: Descendants ?? 0);
    }
}