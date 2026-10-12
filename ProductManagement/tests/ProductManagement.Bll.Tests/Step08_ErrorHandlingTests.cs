using ProductManagement.Bll.Lectures01;
using Xunit;

namespace ProductManagement.Bll.Tests;

/// <summary>Tests for STEP 8 — exceptions.</summary>
public class Step08_ErrorHandlingTests
{
    // --- SWITCH HERE: drop "_Solution" to test your own implementation ----------------
    private static ImportResult ImportBatch(
        IReadOnlyList<string> rows,
        Func<string, ParsedPrice> parse,
        Action<ParsedPrice> apply)
        => Step08_ErrorHandling.ImportBatch_Solution(rows, parse, apply);

    private static ParsedPrice GetRequired(IReadOnlyDictionary<string, ParsedPrice> catalog, string sku)
        => Step08_ErrorHandling.GetRequired_Solution(catalog, sku);

    private static bool TryGet(
        IReadOnlyDictionary<string, ParsedPrice> catalog,
        string? sku,
        out ParsedPrice? product)
        => Step08_ErrorHandling.TryGet_Solution(catalog, sku, out product);
    // ---------------------------------------------------------------------------------

    private static readonly Dictionary<string, ParsedPrice> Catalog =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["VEH-001"] = new ParsedPrice("VEH-001", 18_900m),
            ["TYR-001"] = new ParsedPrice("TYR-001", 109m),
        };

    /// <summary>Parses "SKU;price", throwing the way the real parser would.</summary>
    private static ParsedPrice Parse(string row)
    {
        var parts = row.Split(';');

        if (parts.Length != 2 || !decimal.TryParse(parts[1], out var price))
        {
            throw new InvalidRowException($"'{row}' is not SKU;price");
        }

        return new ParsedPrice(parts[0], price);
    }

    // --- TASK 1 ----------------------------------------------------------------------

    [Fact]
    public void EveryGoodRowIsApplied()
    {
        var applied = new List<ParsedPrice>();
        var rows = new[] { "VEH-001;100", "TYR-001;200" };

        var result = ImportBatch(rows, Parse, applied.Add);

        Assert.Equal(2, result.Succeeded);
        Assert.Equal(0, result.Failed);
        Assert.Equal(2, applied.Count);
    }

    [Fact]
    public void AMalformedRowIsRecordedAndTheBatchContinues()
    {
        var applied = new List<ParsedPrice>();
        var rows = new[] { "VEH-001;100", "rubbish", "TYR-001;200" };

        var result = ImportBatch(rows, Parse, applied.Add);

        Assert.Equal(2, result.Succeeded);
        Assert.Equal(1, result.Failed);
        Assert.Equal(2, result.Failures[0].RowNumber);        // 1-based
        Assert.Contains("Malformed", result.Failures[0].Message, StringComparison.Ordinal);
        Assert.Equal(2, applied.Count);                        // row 3 still ran
    }

    [Fact]
    public void AnUnknownProductIsRecordedAndTheBatchContinues()
    {
        var rows = new[] { "VEH-001;100", "NOPE-001;100", "TYR-001;200" };

        void Apply(ParsedPrice parsed)
        {
            if (!Catalog.ContainsKey(parsed.Sku))
            {
                throw new ProductNotFoundException(parsed.Sku);
            }
        }

        var result = ImportBatch(rows, Parse, Apply);

        Assert.Equal(2, result.Succeeded);
        Assert.Equal(1, result.Failed);
        Assert.Contains("NOPE-001", result.Failures[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnexpectedExceptionAbortsTheBatch()
    {
        var rows = new[] { "VEH-001;100", "TYR-001;200" };

        void Apply(ParsedPrice parsed)
        {
            if (parsed.Sku == "TYR-001")
            {
                throw new InvalidOperationException("the database went away");
            }
        }

        // NOT caught: this means the code or the environment is broken, not the data.
        Assert.Throws<InvalidOperationException>(() => ImportBatch(rows, Parse, Apply));
    }

    [Fact]
    public void AnEmptyBatchSucceedsWithNothing()
    {
        var result = ImportBatch(Array.Empty<string>(), Parse, _ => { });

        Assert.Equal(0, result.Succeeded);
        Assert.Equal(0, result.Failed);
    }

    // --- TASK 2 ----------------------------------------------------------------------

    [Fact]
    public void GetRequiredReturnsTheProduct()
        => Assert.Equal(18_900m, GetRequired(Catalog, "veh-001").Price);

    [Fact]
    public void GetRequiredThrowsProductNotFoundForAMissingProduct()
    {
        var ex = Assert.Throws<ProductNotFoundException>(() => GetRequired(Catalog, "NOPE-001"));

        // The exception carries the DATA, so the catch block does not have to parse text.
        Assert.Equal("NOPE-001", ex.Sku);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetRequiredThrowsArgumentExceptionForABlankSku(string sku)
    {
        // A blank sku is a BUG in the caller, not a missing product. The API layer maps
        // this to 500 and ProductNotFoundException to 404.
        Assert.Throws<ArgumentException>(() => GetRequired(Catalog, sku));
    }

    [Fact]
    public void TryGetFindsTheProduct()
    {
        Assert.True(TryGet(Catalog, "tyr-001", out var product));
        Assert.Equal(109m, product!.Price);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("NOPE-001")]
    public void TryGetNeverThrows(string? sku)
    {
        Assert.False(TryGet(Catalog, sku, out var product));
        Assert.Null(product);
    }

    // --- TASK 3: the four broken catch blocks -----------------------------------------

    [Fact]
    public void TheOriginalParseSwallowsEverything()
    {
        // Garbage silently becomes 0.00 on an invoice.
        Assert.Equal(0m, Step08_ErrorHandling.ParsePriceBug1("rubbish"));
        Assert.Equal(0m, Step08_ErrorHandling.ParsePriceBug1(null!));
    }

    [Fact]
    public void TheFixedParseReportsFailureInstead()
    {
        Assert.False(Step08_ErrorHandling.TryParsePrice_Solution("rubbish", out var price));
        Assert.Equal(0m, price);

        Assert.True(Step08_ErrorHandling.TryParsePrice_Solution("18900.50", out var parsed));
        Assert.Equal(18_900.50m, parsed);
    }

    [Fact]
    public void ThrowExLosesTheOriginalCallSite()
    {
        static void Work() => throw new InvalidOperationException("boom");

        var reset = Assert.Throws<InvalidOperationException>(() => Step08_ErrorHandling.ApplyBug2(Work));
        var preserved = Assert.Throws<InvalidOperationException>(
            () => Step08_ErrorHandling.Apply_Solution(Work, (_, _) => { }));

        // The bare `throw;` keeps Work() in the trace; `throw ex;` throws it away.
        Assert.DoesNotContain("Work", reset.StackTrace ?? string.Empty, StringComparison.Ordinal);
        Assert.Contains("Work", preserved.StackTrace ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFixedApplyLogsAndRethrows()
    {
        var logged = new List<string>();

        Assert.Throws<InvalidOperationException>(() => Step08_ErrorHandling.Apply_Solution(
            () => throw new InvalidOperationException("boom"),
            (message, _) => logged.Add(message)));

        Assert.Single(logged);
    }

    [Fact]
    public void TheFixedSaveHandlesOnlyWhatItCan()
    {
        var logged = new List<string>();

        // Expected: handled and logged.
        Step08_ErrorHandling.Save_Solution(
            () => throw new ProductNotFoundException("NOPE-001"), logged.Add);

        Assert.Single(logged);
        Assert.Contains("NOPE-001", logged[0], StringComparison.Ordinal);

        // Unexpected: propagates.
        Assert.Throws<InvalidOperationException>(() => Step08_ErrorHandling.Save_Solution(
            () => throw new InvalidOperationException("the database went away"), logged.Add));

        Assert.Single(logged);   // nothing new was logged
    }

    [Fact]
    public void TheOriginalSaveSwallowsEverything()
    {
        var logged = new List<string>();

        Step08_ErrorHandling.SaveBug4(
            () => throw new InvalidOperationException("the database went away"), logged.Add);

        // No exception reached the caller, and `save` may have done half its work.
        Assert.Single(logged);
    }
}
