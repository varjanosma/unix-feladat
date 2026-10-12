using ProductManagement.Bll.Lectures01;
using Xunit;

namespace ProductManagement.Bll.Tests;

/// <summary>Tests for STEP 7 — List, Dictionary, HashSet.</summary>
public class Step07_CollectionsTests
{
    // --- SWITCH HERE: drop "_Solution" to test your own implementation ----------------
    private static Dictionary<string, int> TotalsBySku(IEnumerable<StockRow> rows)
        => Step07_Collections.TotalsBySku_Solution(rows);

    private static List<string> NewSkus(IEnumerable<string> existing, IEnumerable<string> incoming)
        => Step07_Collections.NewSkus_Solution(existing, incoming);

    private static Dictionary<string, List<string>> GroupSkus(IEnumerable<StockRow> rows)
        => Step07_Collections.GroupSkus_Solution(rows);
    // ---------------------------------------------------------------------------------

    // --- TASK 1 ----------------------------------------------------------------------

    [Fact]
    public void TotalsAreKeyedBySku()
    {
        var rows = new[] { new StockRow("VEH-001", 3), new StockRow("TYR-001", 64) };

        var totals = TotalsBySku(rows);

        Assert.Equal(2, totals.Count);
        Assert.Equal(3, totals["VEH-001"]);
        Assert.Equal(64, totals["TYR-001"]);
    }

    [Fact]
    public void DuplicateSkusAreSummedNotOverwritten()
    {
        var rows = new[] { new StockRow("VEH-001", 3), new StockRow("VEH-001", 4) };

        Assert.Equal(7, TotalsBySku(rows)["VEH-001"]);
    }

    [Fact]
    public void SkuKeysAreCaseInsensitive()
    {
        var rows = new[] { new StockRow("VEH-001", 3), new StockRow("veh-001", 4) };

        var totals = TotalsBySku(rows);

        Assert.Single(totals);
        Assert.Equal(7, totals["VEH-001"]);
        Assert.Equal(7, totals["veh-001"]);
    }

    [Fact]
    public void AnEmptyInputGivesAnEmptyDictionary()
        => Assert.Empty(TotalsBySku(Array.Empty<StockRow>()));

    // --- TASK 2 ----------------------------------------------------------------------

    [Fact]
    public void OnlyTheUnknownSkusComeBack()
    {
        var existing = new[] { "VEH-001", "TYR-001" };
        var incoming = new[] { "VEH-001", "ENG-001", "TYR-001", "BRK-001" };

        Assert.Equal(new[] { "ENG-001", "BRK-001" }, NewSkus(existing, incoming));
    }

    [Fact]
    public void TheOrderOfIncomingIsPreserved()
    {
        var incoming = new[] { "ZZZ-001", "AAA-001", "MMM-001" };

        Assert.Equal(incoming, NewSkus(Array.Empty<string>(), incoming));
    }

    [Fact]
    public void DuplicatesWithinIncomingAreSuppressed()
    {
        var incoming = new[] { "ENG-001", "ENG-001", "eng-001" };

        Assert.Equal(new[] { "ENG-001" }, NewSkus(Array.Empty<string>(), incoming));
    }

    [Fact]
    public void ComparisonIsCaseInsensitive()
        => Assert.Empty(NewSkus(new[] { "VEH-001" }, new[] { "veh-001" }));

    [Fact]
    public void TheFastAndTheNaiveVersionAgree()
    {
        var existing = new[] { "VEH-001", "TYR-001" };
        var incoming = new[] { "VEH-001", "ENG-001", "BRK-001" };

        Assert.Equal(
            Step07_Collections.NewSkus_Naive(existing, incoming),
            Step07_Collections.NewSkus_Solution(existing, incoming));
    }

    // --- TASK 3 ----------------------------------------------------------------------

    [Fact]
    public void SkusAreGroupedByTheirPrefix()
    {
        var rows = new[]
        {
            new StockRow("VEH-001", 3),
            new StockRow("TYR-001", 64),
            new StockRow("VEH-002", 2),
        };

        var groups = GroupSkus(rows);

        Assert.Equal(2, groups.Count);
        Assert.Equal(new[] { "VEH-001", "VEH-002" }, groups["VEH"]);
        Assert.Equal(new[] { "TYR-001" }, groups["TYR"]);
    }

    [Fact]
    public void ASkuWithoutADashIsItsOwnGroup()
    {
        var groups = GroupSkus(new[] { new StockRow("MISC", 1) });

        Assert.Equal(new[] { "MISC" }, groups["MISC"]);
    }

    [Fact]
    public void GroupingNormalisesTheCase()
    {
        var rows = new[] { new StockRow("veh-001", 1), new StockRow("VEH-002", 1) };

        var groups = GroupSkus(rows);

        Assert.Single(groups);
        Assert.Equal(new[] { "VEH-001", "VEH-002" }, groups["veh"]);
    }

    [Fact]
    public void InputOrderIsKeptInsideAGroup()
    {
        var rows = new[]
        {
            new StockRow("VEH-009", 1),
            new StockRow("VEH-001", 1),
            new StockRow("VEH-005", 1),
        };

        Assert.Equal(new[] { "VEH-009", "VEH-001", "VEH-005" }, GroupSkus(rows)["VEH"]);
    }
}
