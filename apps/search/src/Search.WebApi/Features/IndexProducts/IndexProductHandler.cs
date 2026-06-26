using Elastic.Clients.Elasticsearch;

using Search.Domain;
using Search.WebApi.Configuration;

using Shared.Annotations;

namespace Search.WebApi.Features.IndexProducts;


public class IndexProductsHandler(ElasticsearchClient elasticsearchClient, ILogger<IndexProductsHandler> logger) : IHandler
{
    public async Task<bool> HandleAsync(ProductDocument document, CancellationToken ct)
    {
        try
        {
            var indexResponse = await elasticsearchClient.IndexAsync(document, ct);

            if (!indexResponse.IsValidResponse)
            {
                // 1. Capture the root cause server error from Elasticsearch
                var serverError = indexResponse.ElasticsearchServerError?.Error?.Reason;

                // 2. Capture client-side/network transport exceptions if they occurred
                var debugInfo = indexResponse.DebugInformation;

                logger.LogWarning(
                    "Cannot save document {DocumentId} to index. Reason: {Reason}. DebugInfo: {DebugInfo}",
                    document.Id,
                    serverError ?? "Unknown server error",
                    debugInfo
                );
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Cannot save to index {stack}", ex);
            return false;
        }
    }
}