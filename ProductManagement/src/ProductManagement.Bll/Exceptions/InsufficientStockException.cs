namespace ProductManagement.Bll.Exceptions;

/// <summary>The request is valid, but there is not enough on stock. Carries the data the caller needs.</summary>
public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(string sku, int requested, int available)
        : base($"Not enough stock for {sku}: requested {requested}, available {available}.")
    {
        Sku = sku;
        Requested = requested;
        Available = available;
    }

    public string Sku { get; }

    public int Requested { get; }

    public int Available { get; }
}
