namespace ProductManagement.Transfer;

/// <summary>A single product with the details coming from ProductInfo.</summary>
public sealed record ProductDetailDto
{
    public int Id { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public int StockQuantity { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public int ProductGroupId { get; init; }

    public string ProductGroupName { get; init; } = string.Empty;

    public string? Manufacturer { get; init; }

    public string? Description { get; init; }

    public int? WarrantyMonths { get; init; }

    public decimal? WeightKg { get; init; }

    public string? CountryOfOrigin { get; init; }

    public int TotalSoldQuantity { get; init; }
}
