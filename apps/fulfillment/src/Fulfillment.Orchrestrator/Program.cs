using Fulfillment.Orchrestrator.Consumers;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.Configurations;
using Shared.Exceptions;

using Temporalio.Client;

ILogger logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
}).CreateLogger<Program>();
var builder = Host.CreateApplicationBuilder();

builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = context.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
        if (rabbitConfig == null || string.IsNullOrEmpty(rabbitConfig.Host) || string.IsNullOrEmpty(rabbitConfig.Username) || string.IsNullOrEmpty(rabbitConfig.Password))
        {
            logger.LogError("RabbitMQ configuration is missing or incomplete. Using default localhost settings.");
            throw new MissingConfigurationException("RabbitMq");
        }
        else
        {
            cfg.Host(rabbitConfig.Host, "/", h =>
            {
                h.Username(rabbitConfig.Username);
                h.Password(rabbitConfig.Password);
            });
            cfg.ConfigureEndpoints(context);
        }
    });
});


var client =

builder.Services.AddSingleton<ITemporalClient>(sb =>
{
    return TemporalClient.ConnectAsync(new("localhost:7233")).GetAwaiter().GetResult();
});

var app = builder.Build();
await app.RunAsync();

// var result = await client.ExecuteWorkflowAsync<FulfillmentWorkflow, string>(
//     wf => wf.StartFulfillmentAsync(new FulfillmentWorkflowRequest()),
//     new()
//     {
//         Id = Guid.NewGuid().ToString(),
//         TaskQueue = "checkout-order-fulfillment"
//     }
// );

// Console.WriteLine($"Result: {result}");