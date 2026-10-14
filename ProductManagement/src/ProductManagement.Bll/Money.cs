namespace ProductManagement.Bll;

internal static class Money
{
    /// <summary>Two decimals, 0.5 rounds up. The default (banker's rounding) is a surprise on invoices.</summary>
    public static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
