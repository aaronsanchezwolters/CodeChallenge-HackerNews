using HackerNewsBestStories.Application;
using HackerNewsBestStories.Background;
using HackerNewsBestStories.Infrastructure;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<HackerNewsOptions>()
    .BindConfiguration(HackerNewsOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<BestStoriesOptions>()
    .BindConfiguration(BestStoriesOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient(HackerNewsClient.Name, (provider, httpClient) =>
        httpClient.BaseAddress = provider.GetRequiredService<IOptions<HackerNewsOptions>>().Value.BaseUrl)
    .AddStandardResilienceHandler(); // timeout, retries with backoff and circuit breaker
builder.Services.AddSingleton<IHackerNewsClient, HackerNewsClient>();

builder.Services.AddSingleton<IBestStoriesStore, InMemoryBestStoriesStore>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<BestStoriesRefresher>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "HackerNewsBestStories API"));
}

app.Run();
