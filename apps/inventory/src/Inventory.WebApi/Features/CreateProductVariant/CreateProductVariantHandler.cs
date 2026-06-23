
using Inventory.Domain;
using Inventory.Infrastructure.Persistence;

using Shared.Annotations;
using Shared.Exceptions;

namespace Inventory.WebApi.Features.CreateProductVariant;

public class CreateProductVariantHandler(ProductsRepository repository, IUnitOfWork unitOfWork) : IHandler
{

    public async Task<bool> HandleAsync(CreateProductVariantCommand cmd, CancellationToken token)
    {

        ProductVariant? product = await repository.GetVariantByProductIdAndSkuAsync(cmd.ProductId, cmd.Sku, token);

        if (product != null)
        {
            throw new DuplicateRecordException("Product variant with given sku already exists for the given product.");
        }

        product = new()
        {
            ProductId = cmd.ProductId,
            Sku = cmd.Sku,
            Name = cmd.Name,
            Price = cmd.Price,
            Cost = cmd.Cost,
            ComparePrice = cmd.ComparePrice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await repository.AddVariantAsync(product, token);
        await unitOfWork.SaveChangesAsync(token);
        return true;
    }

}