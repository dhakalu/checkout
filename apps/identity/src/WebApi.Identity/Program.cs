using System.Reflection;
using Scalar.AspNetCore;
using WebApi.Identity.Extentions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using WebApi.Identity.Features.Users.RegisterUser;
using WebApi.Identity.Features.Auth.Token;
using WebApi.Identity.Features.Users.GetUser;
using WebApi.Identity.Features.Clients;

namespace WebApi.Identity;


public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // builder.Services.AddScoped<RegisterUserHandler>();
        // builder.Services.AddScoped<GetUserHandler>();
        // builder.Services.AddScoped<PasswordGrantHandler>();
        // builder.Services.AddScoped<RegisterClientHandler>();

        builder.Services.AddAllHandlers();


        builder.Services.AddSharedErrorHandler(Assembly.GetExecutingAssembly());
        builder.Services.AddDb(builder.Configuration);

        builder.Services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });

        // The generic type string is a placeholder; it isn't utilized by the hash string calculations
        builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseSharedErrorHandling();

        app.MapGet("/health", () =>
        {
            return Results.Ok(new { status = "Healthy" });
        })
        .WithName("GetHealth");

        app.MapAllEndpoints();
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