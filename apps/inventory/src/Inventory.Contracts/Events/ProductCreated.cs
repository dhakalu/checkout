namespace Inventory.Contracts.Events;

public record ProductCreated(
   Guid ProductId,
   string Name,
   string Description,
   string Slug,
   Guid BrandId,
   Guid CategoryId
);