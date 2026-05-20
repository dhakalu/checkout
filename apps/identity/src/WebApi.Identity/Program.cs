using System.Reflection;
using Scalar.AspNetCore;
using Shared.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using WebApi.Identity.Features.Auth.Token;

namespace WebApi.Identity;

public class Program
{
    public static async Task Main(string[] args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddScoped<TokenProvider>();

        builder.Services.AddAllHandlers(assembly);

        builder.Services.AddOpenIddict()
            .AddCore(options =>
                options.UseEntityFrameworkCore()
                .UseDbContext<IdentityDbContext>()
            )
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("/connect/token")
                    .SetAuthorizationEndpointUris("/connect/authorize");
                options.AllowAuthorizationCodeFlow()
                        .AllowRefreshTokenFlow()
                        .AllowPasswordFlow();
                if (builder.Environment.IsDevelopment())
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }

                var aspNetCoreOptions = options.UseAspNetCore()
                .EnableEndUserVerificationEndpointPassthrough();

                if (builder.Environment.IsDevelopment())
                {
                    aspNetCoreOptions.DisableTransportSecurityRequirement();
                }

            });

        builder.Services.AddSharedErrorHandler(Assembly.GetExecutingAssembly());
        builder.Services.AddDb<IdentityDbContext>(builder.Configuration);

        builder.Services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });

        // The generic type string is a placeholder; it isn't utilized by the hash string calculations
        builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();


        var app = builder.Build();
        app.UseRouting();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        // app.UseAuthorization();

        app.UseSharedErrorHandling();

        app.MapGet("/health", () =>
        {
            return Results.Ok(new { status = "Healthy" });
        })
        .WithName("GetHealth");

        app.MapAllEndpoints(assembly);
        await app.StartAsync();
        var serverUrls = app.Urls;
        Console.WriteLine("\n🚀 Application started! Click below to open documentation:");
        foreach (var url in serverUrls)
        {
            Console.WriteLine($"🔗 {url}/scalar/v1");
        }
        Console.WriteLine("\nPress Ctrl+C to shut down.\n");

        await app.WaitForShutdownAsync();
    }
}