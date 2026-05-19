using System.Reflection;
using Scalar.AspNetCore;
using Shared.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace {{cookiecutter.project_namespace}};


public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

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