using System.ComponentModel.DataAnnotations;

namespace HackerNewsBestStories.Infrastructure;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    [Required]
    public Uri BaseUrl { get; set; } = new("https://hacker-news.firebaseio.com/v0/");
}
