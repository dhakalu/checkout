using Inventory.Domain;
using Inventory.Infrastructure.Persistence;

using Shared.Annotations;

namespace Inventory.WebApi.Features.CreateBrand;

public class CreateBrandHandler(BrandRepository repository, IUnitOfWork unitOfWork) : IHandler
{

    public async Task<bool> HandleAsync(CreateBrandCommand cmd, CancellationToken cancellationToken)
    {

        Brand b = new()
        {
            Name = cmd.Name,
            Description = cmd.Description,
            Slug = cmd.Slug,
            WebsiteUrl = cmd.WebsiteUrl,
            IsActive = cmd.IsActive
        };
        await repository.AddAsync(b, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}