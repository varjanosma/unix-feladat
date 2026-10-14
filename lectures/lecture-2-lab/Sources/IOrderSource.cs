using Lecture2Lab.Models;

namespace Lecture2Lab.Sources;

public interface IOrderSource
{
    Task<IReadOnlyList<Order>> LoadAsync(CancellationToken ct = default);
}
