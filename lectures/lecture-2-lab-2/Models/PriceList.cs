namespace Lecture2Lab2.Models;

public sealed record PriceList(string RateDate, IReadOnlyList<ConvertedPrice> Items);
