namespace ProductManagement.Transfer;

/// <summary>A single user together with the purchase history.</summary>
public sealed record UserDetailDto
{
    public int Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public DateTime RegisteredAtUtc { get; init; }

    public decimal TotalSpent { get; init; }

    public IReadOnlyList<PurchaseDto> Purchases { get; init; } = Array.Empty<PurchaseDto>();
}
