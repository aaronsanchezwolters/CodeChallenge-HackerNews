using HackerNewsBestStories.Application;
using HackerNewsBestStories.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HackerNewsBestStories.FunctionalTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FakeHackerNews HackerNews { get; } = new();

    public async Task<HttpClient> CreateClientAfterFirstRefreshAsync()
    {
        var client = CreateClient();
        var store = Services.GetRequiredService<IBestStoriesStore>();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while ((await store.GetBestAsync(1, timeout.Token)).Count == 0)
        {
            await Task.Delay(10, timeout.Token);
        }

        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("HackerNews:BaseUrl", FakeHackerNews.BaseAddress.ToString());
        builder.ConfigureServices(services =>
        {
            services.AddHttpClient(HackerNewsClient.Name).ConfigurePrimaryHttpMessageHandler(() => HackerNews);
        });
    }
}
