namespace ProductManagement.Transfer;

/// <summary>A new order: who buys what.</summary>
public sealed record PlaceOrderRequest
{
    public int UserId { get; init; }

    public string? Note { get; init; }

    public IReadOnlyList<OrderLineRequest> Lines { get; init; } = Array.Empty<OrderLineRequest>();
}

/// <summary>One line of a new order.</summary>
public sealed record OrderLineRequest
{
    public string Sku { get; init; } = string.Empty;

    public int Quantity { get; init; }
}
