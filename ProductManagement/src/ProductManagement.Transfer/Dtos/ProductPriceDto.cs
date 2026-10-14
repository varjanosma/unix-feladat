namespace ProductManagement.Transfer;

/// <summary>The price of a product converted to another currency.</summary>
public sealed record ProductPriceDto
{
    public int ProductId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal PriceEur { get; init; }

    public string Currency { get; init; } = string.Empty;

    public decimal Rate { get; init; }

    public decimal Price { get; init; }

    public string RateDate { get; init; } = string.Empty;
}
