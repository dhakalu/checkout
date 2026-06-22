using System.Collections.ObjectModel;

namespace Inventory.Contracts;

public record GetBrandsResponse(int TotalCount, List<BrandDetail> Data);