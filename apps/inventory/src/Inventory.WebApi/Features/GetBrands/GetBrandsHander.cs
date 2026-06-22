
using Inventory.Contracts;
using Inventory.Infrastructure.Persistence;

using Shared.Annotations;

namespace Inventory.WebApi.Features.GetBrands;

public class GetBrandsHandler(BrandRepository repository) : IHandler
{

    public async Task<GetBrandsResponse> HandleAsync(GetBrandsQuery q, CancellationToken cancellationToken)
    {
        var brands = await repository.GetAllAsync(q.Query, q.Limit, q.Offset, cancellationToken);

        var brandDetails = brands.ConvertAll(b => new BrandDetail(b.Id, b.Name, b.Description, b.Slug, b.WebsiteUrl, b.IsActive));

        return new GetBrandsResponse(100, brandDetails);
    }
}