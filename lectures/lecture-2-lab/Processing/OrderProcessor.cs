using Lecture2Lab.Models;

namespace Lecture2Lab.Processing;

public static class OrderProcessor
{
    public static OrderSummary Summarize(IEnumerable<Order> orders)
    {
        var failures = new List<OrderFailure>();
        var valid = new List<Order>();

        foreach (var order in orders)
        {
            var reason = Validate(order);
            if (reason is null) valid.Add(order);
            else failures.Add(new OrderFailure(order.OrderId, reason));
        }

        var totals = valid
            .GroupBy(o => o.Sku)
            .Select(g => new SkuTotal(g.Key, g.Sum(o => o.Quantity), g.Sum(o => o.Quantity * o.UnitPrice)))
            .OrderByDescending(t => t.Revenue)
            .ToList();

        return new OrderSummary(totals, failures);
    }

    // Bad rows are an expected case: a reason string, not an exception.
    private static string? Validate(Order order) =>
        string.IsNullOrWhiteSpace(order.Sku) ? "SKU is empty"
        : order.Quantity <= 0 ? "Quantity must be positive"
        : null;
}
