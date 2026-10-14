namespace ProductManagement.Transfer;

/// <summary>A product ranked by the quantity sold.</summary>
public sealed record TopProductDto
{
    public int ProductId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int QuantitySold { get; init; }

    public decimal Revenue { get; init; }
}
