namespace ProductManagement.Transfer;

/// <summary>One line of a purchase.</summary>
public sealed record PurchaseItemDto
{
    public int ProductId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal LineTotal { get; init; }
}
