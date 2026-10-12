namespace ProductManagement.Transfer;

/// <summary>A single purchase with the buyer and the lines.</summary>
public sealed record PurchaseDetailDto
{
    public int Id { get; init; }

    public DateTime PurchasedAtUtc { get; init; }

    public string? Note { get; init; }

    public int UserId { get; init; }

    public string UserFullName { get; init; } = string.Empty;

    public string UserEmail { get; init; } = string.Empty;

    public decimal Total { get; init; }

    public IReadOnlyList<PurchaseItemDto> Items { get; init; } = Array.Empty<PurchaseItemDto>();
}
