namespace Lecture2Lab.Models;

public sealed record Order(string OrderId, string Sku, int Quantity, decimal UnitPrice);
