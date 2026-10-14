namespace ProductManagement.Transfer;

/// <summary>A product that is running out.</summary>
public sealed record LowStockItemDto
{
    public int ProductId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int StockQuantity { get; init; }
}
