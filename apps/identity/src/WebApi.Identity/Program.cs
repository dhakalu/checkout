using System.Reflection;
using Scalar.AspNetCore;
using WebApi.Identity.Features.Signup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace WebApi.Identity;


public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddScoped<ISignupService, SignupService>();


        builder.Services.AddSharedErrorHandler(Assembly.GetExecutingAssembly());
        builder.Services.AddDbContext<IdentityDbContext>(options =>
        {
            options.EnableSensitiveDataLogging();
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

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

        SignupEndpoints.MapEndpoints(app);

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