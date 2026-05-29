using Fulfillment.Workflows;

using Temporalio.Client;

var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

var result = await client.ExecuteWorkflowAsync<FulfillmentWorkflow, string>(
    wf => wf.StartFulfillmentAsync(new FulfillmentWorkflowRequest()),
    new()
    {
        Id = Guid.NewGuid().ToString(),
        TaskQueue = "checkout-order-fulfillment"
    }
);

Console.WriteLine($"Result: {result}");