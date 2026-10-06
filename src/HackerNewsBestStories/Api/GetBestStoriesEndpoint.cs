using System.ComponentModel.DataAnnotations;
using HackerNewsBestStories.Application;
using HackerNewsBestStories.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HackerNewsBestStories.Api;

public static class GetBestStoriesEndpoint
{
    public static IEndpointRouteBuilder MapGetBestStories(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/stories/best", HandleAsync)
            .WithSummary("Returns the best Hacker News stories, sorted by score (highest first).")
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<Results<Ok<IReadOnlyList<Story>>, ProblemHttpResult>> HandleAsync(
        [Range(1, int.MaxValue)] int count,
        IBestStoriesStore store,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var stories = await store.GetBestAsync(count, cancellationToken);
        if (stories.Count == 0)
        {
            return TypedResults.Problem(
                detail: "The best stories are still loading. Try again in a few seconds.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return TypedResults.Ok(stories);
    }
}
