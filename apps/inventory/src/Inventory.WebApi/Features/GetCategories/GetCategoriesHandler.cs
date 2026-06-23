using Inventory.Contracts;
using Inventory.Infrastructure.Persistence;

using Shared.Annotations;

namespace Inventory.WebApi.Features.GetCategories;

public class GetCategoriesHandler(ProductCategoryRepository repository) : IHandler
{


    public async Task<GetCategoriesResponse> HandleAsync(GetCategoriesQuery q, CancellationToken ct)
    {
        var categories = await repository.GetAllAsync(q.Limit, q.Offset, q.Query, ct);
        var categoryDetailList = categories.ConvertAll(x => new CagetoryDetail(x.Id, x.Name, x.Description, x.Slug, x.IsActive));
        return new GetCategoriesResponse(100, categoryDetailList);
    }
}