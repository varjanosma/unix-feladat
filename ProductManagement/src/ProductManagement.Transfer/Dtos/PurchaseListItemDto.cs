namespace ProductManagement.Transfer;

/// <summary>One row of the purchase list.</summary>
public sealed record PurchaseListItemDto
{
    public int Id { get; init; }

    public DateTime PurchasedAtUtc { get; init; }

    public string? Note { get; init; }

    public int UserId { get; init; }

    public string UserFullName { get; init; } = string.Empty;

    public int ItemCount { get; init; }

    public int TotalQuantity { get; init; }

    public decimal Total { get; init; }
}
