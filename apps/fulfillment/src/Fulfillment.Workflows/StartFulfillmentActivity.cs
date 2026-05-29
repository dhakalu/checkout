using Temporalio.Activities;

namespace Fulfillment.Workflows;

public class StartFulfillmentActivity
{
    [Activity]
    public static async Task<string> StartAsync(FulfillmentWorkflowRequest request)
    {
        return "done!";
    }
}