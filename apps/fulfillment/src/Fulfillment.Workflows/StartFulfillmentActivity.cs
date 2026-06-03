using Microsoft.Extensions.Logging;

using Temporalio.Activities;

namespace Fulfillment.Workflows;

public class StartFulfillmentActivity(ILogger<StartFulfillmentActivity> logger)
{
    [Activity]
    public async Task<string> StartAsync(FulfillmentWorkflowRequest request)
    {
        logger.LogInformation("Starting fulfillment process.");
        return $"Fulfillment process started for OrderId: {request.OrderId}, CustomerId: {request.CustomerId}";
    }
}