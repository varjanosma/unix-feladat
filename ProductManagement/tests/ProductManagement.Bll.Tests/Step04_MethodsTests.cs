using ProductManagement.Bll.Lectures01;
using Xunit;

namespace ProductManagement.Bll.Tests;

/// <summary>Tests for STEP 4 — parameters, the Try pattern, overloads.</summary>
public class Step04_MethodsTests
{
    // --- SWITCH HERE: drop "_Solution" to test your own implementation ----------------
    private static decimal GrossPrice(decimal net, decimal vatPercent = 27m)
        => Step04_Methods.GrossPrice_Solution(net, vatPercent);

    private static bool TryParseSku(string? sku, out string groupCode, out int number)
        => Step04_Methods.TryParseSku_Solution(sku, out groupCode, out number);

    private static string Summarise(OrderRequest request) => Step04_Methods.Summarise_Solution(request);
    // ---------------------------------------------------------------------------------

    // --- TASK 1 ----------------------------------------------------------------------

    [Fact]
    public void GrossPriceUsesTwentySevenPercentByDefault()
        => Assert.Equal(127.00m, GrossPrice(100m));

    [Fact]
    public void GrossPriceAcceptsAnotherRate()
        => Assert.Equal(105.00m, GrossPrice(100m, 5m));

    [Fact]
    public void GrossPriceRoundsAwayFromZero()
    {
        // 10.01 * 1.27 = 12.7127 -> 12.71
        // 10.02 * 1.27 = 12.7254 -> 12.73
        Assert.Equal(12.71m, GrossPrice(10.01m));
        Assert.Equal(12.73m, GrossPrice(10.02m));

        // The midpoint case: banker's rounding would give 2, not 3.
        Assert.Equal(3m, Math.Round(2.5m, MidpointRounding.AwayFromZero));
    }

    // --- TASK 2 ----------------------------------------------------------------------

    [Fact]
    public void TryParseSkuSplitsAValidSku()
    {
        Assert.True(TryParseSku("VEH-001", out var code, out var number));
        Assert.Equal("VEH", code);
        Assert.Equal(1, number);
    }

    [Fact]
    public void TryParseSkuUppercasesTheGroupCode()
    {
        Assert.True(TryParseSku("veh-042", out var code, out var number));
        Assert.Equal("VEH", code);
        Assert.Equal(42, number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("VEH001")]        // no dash
    [InlineData("VEH-001-X")]     // three parts
    [InlineData("VEH-abc")]       // not a number
    public void TryParseSkuRejectsAnythingElse(string? sku)
    {
        Assert.False(TryParseSku(sku, out var code, out var number));

        // Both out parameters must still be assigned on the failure path.
        Assert.Equal(string.Empty, code);
        Assert.Equal(0, number);
    }

    // --- TASK 3 ----------------------------------------------------------------------

    [Fact]
    public void SummariseWithoutExpress()
    {
        var request = new OrderRequest { UserId = 1, Sku = "VEH-001", Quantity = 2, UnitPrice = 18_900m };

        Assert.Equal("user 1: VEH-001 x2 = 37800.00", Summarise(request));
    }

    [Fact]
    public void SummariseWithExpress()
    {
        var request = new OrderRequest
        {
            UserId = 1,
            Sku = "VEH-001",
            Quantity = 2,
            UnitPrice = 18_900m,
            Express = true,
        };

        Assert.Equal("user 1: VEH-001 x2 = 37800.00 + express 4900.00 = 42700.00", Summarise(request));
    }

    [Fact]
    public void QuantityDefaultsToOne()
    {
        var request = new OrderRequest { UserId = 7, Sku = "TYR-001", UnitPrice = 109m };

        Assert.Equal("user 7: TYR-001 x1 = 109.00", Summarise(request));
    }

    // --- TASK 4: overload resolution --------------------------------------------------

    [Fact]
    public void OverloadResolutionPicksTheExactMatch()
    {
        Assert.Equal(100m, Step04_Methods.Apply(100m));          // A
        Assert.Equal(90m, Step04_Methods.Apply(100m, 10));       // B - int literal
        Assert.Equal(90m, Step04_Methods.Apply(100m, 10m));      // C - decimal literal
        Assert.Equal(90m, Step04_Methods.Apply(100, 10));        // B again
    }

    [Fact]
    public void OneMethodWithADefaultDoesTheSameJob()
    {
        Assert.Equal(100m, Step04_Methods.Apply_Solution(100m));
        Assert.Equal(90m, Step04_Methods.Apply_Solution(100m, 10m));
    }
}
