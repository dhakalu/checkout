using System.Reflection;

using Elastic.Clients.Elasticsearch;

using MassTransit;

using Microsoft.Extensions.Options;

using Search.Domain;
using Search.WebApi.Configuration;
using Search.WebApi.Consumers;

using Shared.Configurations;
using Shared.DependencyInjection;
using Shared.Exceptions;

namespace Search.WebApi;

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
        builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMq"));
        builder.Services.Configure<ElasticSearchConfiguration>(builder.Configuration.GetSection("ElasticSearch"));


        builder.Services.AddSingleton<ElasticsearchClient>((ctx) =>
        {
            ElasticSearchConfiguration esConfig = ctx.GetRequiredService<IOptions<ElasticSearchConfiguration>>().Value;
            if (esConfig == null || string.IsNullOrEmpty(esConfig.Host))
                throw new MissingConfigurationException("ElasticSearch");
            var esSettings = new ElasticsearchClientSettings(new Uri(esConfig.Host))
                .DefaultMappingFor<ProductDocument>(m => m.IndexName("products"));

            return new ElasticsearchClient(esSettings);
        });

        builder.Services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<ProductCreatedConsumer>();
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
                    rabbitCfg.ConfigureEndpoints(context);
                });
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