using Microsoft.EntityFrameworkCore;
using ProductManagement.Bll.Exceptions;
using ProductManagement.Dal;
using ProductManagement.Dal.Entities;
using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

internal sealed class OrderService : IOrderService
{
    private readonly ProductManagementDbContext _dbContext;

    public OrderService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        var lines = request.Lines
            .Select(l => new { Sku = l.Sku.Trim().ToUpperInvariant(), l.Quantity })
            .ToList();

        // 1. Validate everything first and collect ALL problems: the caller can fix them in one go.
        var errors = new List<string>();

        if (lines.Count == 0)
        {
            errors.Add("The order has no lines.");
        }

        errors.AddRange(lines
            .Where(l => l.Quantity <= 0)
            .Select(l => $"{l.Sku}: quantity must be positive."));

        errors.AddRange(lines
            .GroupBy(l => l.Sku)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key}: listed more than once."));

        if (!await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            errors.Add($"User {request.UserId} does not exist.");
        }

        var skus = lines.Select(l => l.Sku).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(p => skus.Contains(p.Sku))
            .ToDictionaryAsync(p => p.Sku, cancellationToken);

        foreach (var sku in skus)
        {
            if (!products.TryGetValue(sku, out var product))
            {
                errors.Add($"{sku}: unknown product.");
            }
            else if (!product.IsActive)
            {
                errors.Add($"{sku}: not for sale.");
            }
        }

        if (errors.Count > 0)
        {
            throw new OrderValidationException(errors);
        }

        // 2. Business rule: a valid request can still fail if there is not enough stock.
        foreach (var line in lines)
        {
            var product = products[line.Sku];
            if (product.StockQuantity < line.Quantity)
            {
                throw new InsufficientStockException(line.Sku, line.Quantity, product.StockQuantity);
            }
        }

        // 3. Only now change anything: nothing is half-done if a check fails.
        var purchase = new Purchase
        {
            UserId = request.UserId,
            Note = request.Note,
            PurchasedAtUtc = DateTime.UtcNow,
        };

        foreach (var line in lines)
        {
            var product = products[line.Sku];
            product.StockQuantity -= line.Quantity;
            purchase.Items.Add(new PurchaseItem
            {
                ProductId = product.Id,
                Quantity = line.Quantity,
                UnitPrice = product.Price,
            });
        }

        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return purchase.Id;
    }
}
