namespace Lecture2Lab2.Rates;

public sealed class ExchangeRateUnavailableException : Exception
{
    public ExchangeRateUnavailableException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
