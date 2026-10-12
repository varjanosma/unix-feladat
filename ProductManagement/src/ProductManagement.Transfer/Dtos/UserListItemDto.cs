namespace ProductManagement.Transfer;

/// <summary>One row of the user list, with a few aggregated numbers.</summary>
public sealed record UserListItemDto
{
    public int Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public DateTime RegisteredAtUtc { get; init; }

    public int PurchaseCount { get; init; }

    public decimal TotalSpent { get; init; }
}
