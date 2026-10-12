using ProductManagement.Bll.Lectures01;
using Xunit;

namespace ProductManagement.Bll.Tests;

/// <summary>Tests for STEP 2 — value vs reference, nullable, strings.</summary>
public class Step02_TypeSystemTests
{
    // --- SWITCH HERE: drop "_Solution" to test your own implementation ----------------
    private static CleanRow Clean(RawRow raw) => Step02_TypeSystem.Clean_Solution(raw);

    private static string RenderList(IEnumerable<CleanRow> rows) => Step02_TypeSystem.RenderList_Solution(rows);

    private static ProductState ApplyPatch(ProductState current, ProductPatch patch)
        => Step02_TypeSystem.ApplyPatch_Solution(current, patch);
    // ---------------------------------------------------------------------------------

    // --- TASK 1 ----------------------------------------------------------------------

    [Fact]
    public void CleanTrimsAndUppercasesTheSku()
    {
        var result = Clean(new RawRow("  veh-001 ", "  Volkswagen Golf  ", "18900.50"));

        Assert.True(result.IsValid);
        Assert.Equal("VEH-001", result.Sku);
        Assert.Equal("Volkswagen Golf", result.Name);   // trimmed, casing kept
        Assert.Equal(18_900.50m, result.Price);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CleanRejectsAMissingSku(string? sku)
    {
        var result = Clean(new RawRow(sku, "Golf", "100"));

        Assert.False(result.IsValid);
        Assert.Contains("Sku", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CleanRejectsAMissingName()
    {
        var result = Clean(new RawRow("VEH-001", "  ", "100"));

        Assert.False(result.IsValid);
        Assert.Contains("Name", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("-1")]
    public void CleanRejectsABadPrice(string? price)
    {
        var result = Clean(new RawRow("VEH-001", "Golf", price));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CleanParsesWithTheInvariantCulture()
    {
        // A dot is the decimal separator, whatever the machine is set to.
        var result = Clean(new RawRow("VEH-001", "Golf", "1899.95"));

        Assert.True(result.IsValid);
        Assert.Equal(1_899.95m, result.Price);
    }

    // --- TASK 2 ----------------------------------------------------------------------

    [Fact]
    public void RenderListPadsEveryColumn()
    {
        var rows = new[] { CleanRow.Ok("VEH-001", "Volkswagen Golf", 18_900m) };

        var line = RenderList(rows).TrimEnd('\r', '\n');

        Assert.Equal(52, line.Length);                       // 12 + 30 + 10
        Assert.StartsWith("VEH-001     ", line, StringComparison.Ordinal);
        Assert.EndsWith("  18900.00", line, StringComparison.Ordinal);
    }

    [Fact]
    public void RenderListSkipsInvalidRows()
    {
        var rows = new[]
        {
            CleanRow.Ok("VEH-001", "Golf", 100m),
            CleanRow.Fail("broken"),
            CleanRow.Ok("TYR-001", "Tyre", 200m),
        };

        var lines = RenderList(rows).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Contains("VEH-001", lines[0], StringComparison.Ordinal);
        Assert.Contains("TYR-001", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public void RenderListOfNothingIsEmpty()
        => Assert.Equal(string.Empty, RenderList(Array.Empty<CleanRow>()));

    // --- TASK 3 ----------------------------------------------------------------------

    [Fact]
    public void PatchWithOnlyNullsChangesNothing()
    {
        var current = new ProductState("Golf", 100m, 5);

        var updated = ApplyPatch(current, new ProductPatch(null, null, null));

        Assert.Equal(current, updated);
    }

    [Fact]
    public void EmptyNameMeansClearedNotUnchanged()
    {
        var current = new ProductState("Golf", 100m, 5);

        var updated = ApplyPatch(current, new ProductPatch("", null, null));

        Assert.Equal("(no name)", updated.Name);
    }

    [Fact]
    public void PatchedValuesAreUsedAndTrimmed()
    {
        var current = new ProductState("Golf", 100m, 5);

        var updated = ApplyPatch(current, new ProductPatch("  Octavia  ", 250.5m, 0));

        Assert.Equal("Octavia", updated.Name);
        Assert.Equal(250.5m, updated.Price);
        Assert.Equal(0, updated.Quantity);    // 0 is a real value, not "missing"
    }

    [Fact]
    public void ApplyPatchDoesNotMutateTheOriginal()
    {
        var current = new ProductState("Golf", 100m, 5);

        ApplyPatch(current, new ProductPatch("Octavia", 999m, 1));

        Assert.Equal("Golf", current.Name);
        Assert.Equal(100m, current.Price);
        Assert.Equal(5, current.Quantity);
    }
}
