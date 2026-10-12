namespace ProductManagement.Transfer;

/// <summary>A purchase with its lines.</summary>
public sealed record PurchaseDto
{
    public int Id { get; init; }

    public DateTime PurchasedAtUtc { get; init; }

    public string? Note { get; init; }

    public decimal Total { get; init; }

    public IReadOnlyList<PurchaseItemDto> Items { get; init; } = Array.Empty<PurchaseItemDto>();
}
