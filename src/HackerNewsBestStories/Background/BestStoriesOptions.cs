using System.ComponentModel.DataAnnotations;

namespace HackerNewsBestStories.Background;

public sealed class BestStoriesOptions
{
    public const string SectionName = "BestStories";

    /// <summary>How often the snapshot is rebuilt.</summary>
    [Range(typeof(TimeSpan), "00:00:05", "1.00:00:00")]
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Maximum number of concurrent requests sent to Hacker News during a refresh.</summary>
    [Range(1, 100)]
    public int MaxConcurrentRequests { get; set; } = 10;
}
