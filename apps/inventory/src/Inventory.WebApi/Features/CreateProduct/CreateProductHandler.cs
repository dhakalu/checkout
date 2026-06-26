
using Inventory.Contracts.Events;
using Inventory.Domain;
using Inventory.Infrastructure.Persistence;

using MassTransit;

using Shared.Annotations;
using Shared.Exceptions;

namespace Inventory.WebApi.Features.CreateProduct;

public class CreateProductHandler(
    ProductsRepository repository,
    BrandRepository brandRepository,
    ProductCategoryRepository productCategoryRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IHandler
{

    public async Task<bool> HandleAsync(CreateProductCommand cmd, CancellationToken token)
    {
        Product? product = await repository.GetByBrandIdAndNameAsync(cmd.BrandId, cmd.Name, token);

        if (product != null)
        {
            throw new DuplicateRecordException("Product with given already exists for the given brand.");
        }

        Brand brand = await brandRepository.GetByIdAsync(cmd.BrandId, token) ??
             throw new RecordNotFoundException($"Brand '{cmd.BrandId}' was not found.");
        ProductCategory category = await productCategoryRepository.GetByIdAsync(cmd.CategoryId, token) ??
            throw new RecordNotFoundException($"Category '{cmd.CategoryId}' was not found.");


        product = new()
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
        await publishEndpoint.Publish<ProductCreated>(new(product.Id, product.Name, product.Description, product.Slug, product.BrandId, brand.Name, product.CategoryId, category.Name), token);
        await unitOfWork.SaveChangesAsync(token);
        return true;
    }

}