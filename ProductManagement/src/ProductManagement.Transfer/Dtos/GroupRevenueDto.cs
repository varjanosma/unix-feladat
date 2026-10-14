namespace ProductManagement.Transfer;

/// <summary>Sales of one product group.</summary>
public sealed record GroupRevenueDto
{
    public int ProductGroupId { get; init; }

    public string ProductGroupName { get; init; } = string.Empty;

    public int QuantitySold { get; init; }

    public decimal Revenue { get; init; }
}
