using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


RateLimitOptions rateLimitOptions = builder.Configuration.GetSection("RateLimit").Get<RateLimitOptions>() ?? throw new MissingFieldException("RateLimit configuration section is missing or invalid.");

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Fixed", opts =>
    {
        opts.Window = TimeSpan.FromSeconds(rateLimitOptions.WindowInSeconds);
        opts.PermitLimit = rateLimitOptions.PermitLimit;
        opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opts.QueueLimit = Int32.MaxValue;
    });
});

var app = builder.Build();

app.UseRateLimiter();
app.MapReverseProxy();

app.Run();


sealed class RateLimitOptions
{
    public int PermitLimit { get; set; }
    public Int32 WindowInSeconds { get; set; }
}