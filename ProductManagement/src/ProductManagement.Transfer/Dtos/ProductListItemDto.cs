namespace ProductManagement.Transfer;

/// <summary>One row of the product list.</summary>
public sealed record ProductListItemDto
{
    public int Id { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public int StockQuantity { get; init; }

    public bool IsActive { get; init; }

    public int ProductGroupId { get; init; }

    public string ProductGroupName { get; init; } = string.Empty;
}
