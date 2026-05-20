using System.Reflection;
using Scalar.AspNetCore;
using Shared.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using {{cookiecutter.infra_namespace}}.Persistence;

namespace {{cookiecutter.webapi_namespace}};

public class Program
{
    public static async Task Main(string[] args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddSharedErrorHandler(assembly);
        builder.Services.AddAllHandlers(assembly);
        builder.Services.AddDb<{{cookiecutter.__entity_name}}DbContext>(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(); 
        }

        app.UseHttpsRedirection();
        app.UseExceptionHandler();

        app.MapGet("/health", () =>
        {
            return Results.Ok(new { status = "Healthy" });
        })
        .WithName("GetHealth");

        await StartAsync(app);
    }

    private static async Task StartAsync(WebApplication app)
    {
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