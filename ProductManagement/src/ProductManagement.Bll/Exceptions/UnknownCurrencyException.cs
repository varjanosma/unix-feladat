namespace ProductManagement.Bll.Exceptions;

public sealed class UnknownCurrencyException : Exception
{
    public UnknownCurrencyException(string currency)
        : base($"Unknown currency: '{currency}'.")
        => Currency = currency;

    public string Currency { get; }
}
