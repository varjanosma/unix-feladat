namespace Lecture2Lab.Models;

public sealed record OrderSummary(IReadOnlyList<SkuTotal> Totals, IReadOnlyList<OrderFailure> Failures);
