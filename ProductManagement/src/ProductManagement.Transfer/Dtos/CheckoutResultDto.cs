namespace ProductManagement.Transfer;

/// <summary>Outcome of a checkout: the new purchase and its total in the requested currency.</summary>
public sealed record CheckoutResultDto
{
    public int PurchaseId { get; init; }

    public decimal TotalEur { get; init; }

    public string Currency { get; init; } = string.Empty;

    public decimal Rate { get; init; }

    public decimal Total { get; init; }

    public string InvoiceUrl { get; init; } = string.Empty;
}
