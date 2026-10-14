using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface IOrderService
{
    /// <summary>Creates a purchase and decreases the stock. Returns the id of the new purchase.</summary>
    /// <exception cref="Exceptions.OrderValidationException">The request is invalid.</exception>
    /// <exception cref="Exceptions.InsufficientStockException">A product does not have enough stock.</exception>
    Task<int> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default);
}
