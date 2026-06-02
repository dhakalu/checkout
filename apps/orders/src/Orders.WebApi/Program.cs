using System.Reflection;

using MassTransit;
using MassTransit.Configuration;
using MassTransit.RabbitMqTransport.Configuration;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Orders.Infrastructure.Persistence;

using Scalar.AspNetCore;

using Shared.DependencyInjection;

namespace Orders.WebApi;


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
        builder.Services.AddDb<OrderDbContext>(builder.Configuration);
        builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMq"));
        builder.Services.AddMassTransit(x =>
        {
            if (builder.Environment.IsDevelopment())
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitConfig = context.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
                    cfg.Host(rabbitConfig.Host, "/", h =>
                    {
                        h.Username(rabbitConfig.Username);
                        h.Password(rabbitConfig.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            }
            x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

        });

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
        app.MapAllEndpoints(assembly);

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

internal class RabbitMqConfiguration
{
    internal string Host { get; set; } = default!;
    internal string Username { get; set; } = default!;
    internal string Password { get; set; } = default!;
}