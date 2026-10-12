using ProductManagement.Bll.Lectures01;
using Xunit;

namespace ProductManagement.Bll.Tests;

/// <summary>Tests for STEP 5 — encapsulation and visibility.</summary>
public class Step05_ClassesAndVisibilityTests
{
    // --- SWITCH HERE ------------------------------------------------------------------
    // The basket is a CLASS, so switching means changing the two lines below to
    // `ShoppingBasket` and `PriceCalculator` once you have implemented them.
    private static ShoppingBasket_Solution NewBasket() => new();

    private static PriceCalculator_Solution NewCalculator(decimal vatPercent = 27m) => new(vatPercent);
    // ---------------------------------------------------------------------------------

    // --- TASK 1 ----------------------------------------------------------------------

    [Fact]
    public void ANewBasketIsEmpty()
    {
        var basket = NewBasket();

        Assert.Empty(basket.Lines);
        Assert.Equal(0, basket.LineCount);
        Assert.Equal(0m, basket.Total);
    }

    [Fact]
    public void AddingLinesUpdatesTheComputedTotals()
    {
        var basket = NewBasket();

        basket.Add("VEH-001", 1, 18_900m);
        basket.Add("TYR-001", 4, 109m);

        Assert.Equal(2, basket.LineCount);
        Assert.Equal(19_336m, basket.Total);
    }

    [Fact]
    public void AddingTheSameSkuMergesInsteadOfDuplicating()
    {
        var basket = NewBasket();

        basket.Add("TYR-001", 2, 109m);
        basket.Add("tyr-001", 2, 120m);      // case-insensitive, and the new price wins

        Assert.Equal(1, basket.LineCount);
        Assert.Equal(4, basket.Lines[0].Quantity);
        Assert.Equal(120m, basket.Lines[0].UnitPrice);
        Assert.Equal(480m, basket.Total);
    }

    [Fact]
    public void TheSkuIsStoredUppercased()
    {
        var basket = NewBasket();

        basket.Add("  veh-001  ", 1, 100m);

        Assert.Equal("VEH-001", basket.Lines[0].Sku);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankSkuIsRejected(string? sku)
        => Assert.Throws<ArgumentException>(() => NewBasket().Add(sku!, 1, 100m));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ANonPositiveQuantityIsRejected(int quantity)
        => Assert.Throws<ArgumentOutOfRangeException>(() => NewBasket().Add("VEH-001", quantity, 100m));

    [Fact]
    public void ANegativePriceIsRejected()
        => Assert.Throws<ArgumentOutOfRangeException>(() => NewBasket().Add("VEH-001", 1, -1m));

    [Fact]
    public void RemoveReportsWhetherItRemovedAnything()
    {
        var basket = NewBasket();
        basket.Add("VEH-001", 1, 100m);

        Assert.True(basket.Remove("veh-001"));
        Assert.False(basket.Remove("veh-001"));
        Assert.Equal(0, basket.LineCount);
    }

    [Fact]
    public void TheCallerCannotModifyTheLinesItGetsBack()
    {
        var basket = NewBasket();
        basket.Add("VEH-001", 1, 100m);

        // AsReadOnly returns a wrapper, not the list - the cast gives null.
        Assert.Null(basket.Lines as List<BasketLine>);
        Assert.IsAssignableFrom<IReadOnlyList<BasketLine>>(basket.Lines);
    }

    [Fact]
    public void TotalAlwaysAgreesWithTheLines()
    {
        var basket = NewBasket();
        basket.Add("A-001", 3, 10m);
        basket.Add("A-002", 2, 20m);

        Assert.Equal(basket.Lines.Sum(line => line.LineTotal), basket.Total);
    }

    // --- TASK 2 ----------------------------------------------------------------------

    [Fact]
    public void CalculateGrossUsesTheConfiguredVat()
    {
        Assert.Equal(127m, NewCalculator().CalculateGross(100m));
        Assert.Equal(105m, NewCalculator(5m).CalculateGross(100m));
    }

    [Fact]
    public void TheVatIsValidatedAtConstruction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NewCalculator(-1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => NewCalculator(101m));
    }

    [Fact]
    public void TheVatCannotBeChangedAfterwards()
    {
        var type = typeof(PriceCalculator_Solution);
        var property = type.GetProperty(nameof(PriceCalculator_Solution.VatPercent))!;

        Assert.False(property.CanWrite);
    }

    [Fact]
    public void RoundingIsNotPartOfThePublicContract()
    {
        var type = typeof(PriceCalculator_Solution);

        Assert.Null(type.GetMethod("Round"));                 // private
        Assert.Null(type.GetMethod("ConvertInternalUnit"));   // private

        // The original leaked both of them:
        Assert.NotNull(typeof(PriceCalculator).GetMethod("Round"));
    }
}
