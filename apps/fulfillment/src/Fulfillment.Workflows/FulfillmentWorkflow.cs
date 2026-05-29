using Temporalio.Workflows;

namespace Fulfillment.Workflows;

[Workflow]
public class FulfillmentWorkflow
{

    [WorkflowRun]
    public async Task<string> StartFulfillmentAsync(FulfillmentWorkflowRequest request)
    {

        return await Workflow.ExecuteActivityAsync<string>(
            () => StartFulfillmentActivity.StartAsync(request),
            new()
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(2)
            }
        );
    }
}