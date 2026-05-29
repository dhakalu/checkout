using Fulfillment.Workflows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Temporalio.Client;
using Temporalio.Worker;

var builder = Host.CreateApplicationBuilder();
builder.Logging.AddConsole();

CancellationTokenSource cancellationTokenSource = new();


var cancellationToken = cancellationTokenSource.Token;

var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

var worker = new TemporalWorker(client,
    new TemporalWorkerOptions("checkout-order-fulfillment")
    .AddWorkflow<FulfillmentWorkflow>()
    .AddActivity(StartFulfillmentActivity.StartAsync)
);

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    await worker.ExecuteAsync(cancellationToken);
}
catch (Exception ex)
{
    logger.LogError(ex, "Cannot execute work");
}