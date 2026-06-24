using System.Reflection;

using Inventory.Infrastructure.Persistence;

using MassTransit;
using MassTransit.RabbitMqTransport.Configuration;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Shared.Configurations;
using Shared.DependencyInjection;
using Shared.Exceptions;

namespace Inventory.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddSharedErrorHandler(assembly);
        builder.Services.AddAllHandlers(assembly);
        builder.Services.AddDb<InventoryDbContext>(builder.Configuration);
        builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMq"));

        builder.Services.AddMassTransit(cfg =>
        {
            cfg.UsingRabbitMq((context, rabbitCfg) =>
            {
                RabbitMqConfiguration? mqConfiguration = context.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
                if (rabbitCfg == null
                    || string.IsNullOrEmpty(mqConfiguration.Username)
                    || string.IsNullOrEmpty(mqConfiguration.Host)
                    || string.IsNullOrEmpty(mqConfiguration.Password)
                )
                {
                    throw new MissingConfigurationException("RabbitMq");
                }
                rabbitCfg.Host(mqConfiguration.Host, "/", (hostConfigurator) =>
                {
                    hostConfigurator.Username(mqConfiguration.Username);
                    hostConfigurator.Password(mqConfiguration.Password);
                });
            });
            cfg.AddEntityFrameworkOutbox<InventoryDbContext>(outboxCfg =>
            {
                outboxCfg.UseBusOutbox();
                outboxCfg.UsePostgres();
            });
        });

        var app = builder.Build();
        app.MapAllEndpoints(assembly);

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