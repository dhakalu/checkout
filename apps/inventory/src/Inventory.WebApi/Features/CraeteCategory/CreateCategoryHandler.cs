
using Inventory.Domain;
using Inventory.Infrastructure.Persistence;

using Shared.Annotations;

namespace Inventory.WebApi.Features.CraeteCategory;

public class CreateCategoryHandler(ProductCategoryRepository repository, IUnitOfWork unitOfWork) : IHandler
{


    public async Task<bool> HandleAsync(CreateCategoryCommand cmd, CancellationToken cancellationToken)
    {
        ProductCategory category = new()
        {
            Name = cmd.Name,
            Description = cmd.Description,
            Slug = cmd.Slug,
            IsActive = cmd.IsActive
        };
        await repository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}