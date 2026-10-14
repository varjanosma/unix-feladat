namespace ProductManagement.Bll.Rates;

/// <summary>1 <see cref="From"/> = <see cref="Rate"/> <see cref="To"/>, as of <see cref="Date"/>.</summary>
public sealed record ExchangeRate(string From, string To, decimal Rate, string Date);
