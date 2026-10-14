using Microsoft.EntityFrameworkCore;
using ProductManagement.Bll.Rates;
using ProductManagement.Dal;
using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

internal sealed class PriceService : IPriceService
{
    private readonly ProductManagementDbContext _dbContext;
    private readonly IExchangeRateProvider _rateProvider;

    public PriceService(ProductManagementDbContext dbContext, IExchangeRateProvider rateProvider)
    {
        _dbContext = dbContext;
        _rateProvider = rateProvider;
    }

    public async Task<ProductPriceDto?> GetPriceAsync(int productId, string currency, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.Id, p.Sku, p.Name, p.Price })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return null;
        }

        // Two awaits in a row: the database first, then the network. The thread is free during both.
        var rate = await GetRateFromEurAsync(_rateProvider, currency, cancellationToken);

        return new ProductPriceDto
        {
            ProductId = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            PriceEur = product.Price,
            Currency = rate.To,
            Rate = rate.Rate,
            Price = Money.Round(product.Price * rate.Rate),
            RateDate = rate.Date,
        };
    }

    /// <summary>EUR -> EUR needs no network call.</summary>
    internal static async Task<ExchangeRate> GetRateFromEurAsync(
        IExchangeRateProvider rateProvider,
        string currency,
        CancellationToken cancellationToken)
    {
        var target = currency.Trim().ToUpperInvariant();

        return target == "EUR"
            ? new ExchangeRate("EUR", "EUR", 1m, DateTime.UtcNow.ToString("yyyy-MM-dd"))
            : await rateProvider.GetRateAsync("EUR", target, cancellationToken);
    }
}
