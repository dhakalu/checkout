
using Inventory.Domain;
using Inventory.Infrastructure.Persistence;

using Shared.Annotations;

namespace Inventory.WebApi.Features.CreateProduct;

public class CreateProductHandler(ProductsRepository repository, IUnitOfWork unitOfWork) : IHandler
{

    public async Task<bool> HandleAsync(CreateProductCommand cmd, CancellationToken token)
    {
        Product product = new()
        {
            CategoryId = cmd.CategoryId,
            BrandId = cmd.BrandId,
            Name = cmd.Name,
            Description = cmd.Description,
            Slug = cmd.Slug,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await repository.AddAsync(product, token);
        await unitOfWork.SaveChangesAsync(token);
        return true;
    }

}