namespace ProductManagement.Bll.Exceptions;

/// <summary>The rate service could not be reached or gave an unusable answer.</summary>
public sealed class ExchangeRateUnavailableException : Exception
{
    public ExchangeRateUnavailableException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
