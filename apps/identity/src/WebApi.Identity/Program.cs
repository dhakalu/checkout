using Scalar.AspNetCore;

namespace WebApi.Identity;


public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(); 
        }

        app.UseHttpsRedirection();

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