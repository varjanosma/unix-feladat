using System.Text;
using ProductManagement.Bll.Lectures01;
using Xunit;

namespace ProductManagement.Bll.Tests;

/// <summary>
/// Tests for STEP 6 — file handling. Every test works in its own temporary directory,
/// which is deleted afterwards. Nothing here touches a path outside the temp folder.
/// </summary>
public sealed class Step06_FileHandlingTests : IDisposable
{
    // --- SWITCH HERE: drop "_Solution" to test your own implementation ----------------
    private static List<StockLine> ReadStockFile(string directory, string fileName)
        => Step06_FileHandling.ReadStockFile_Solution(directory, fileName);

    private static string WriteStockFile(string directory, string fileName, IEnumerable<StockLine> lines, DateOnly date)
        => Step06_FileHandling.WriteStockFile_Solution(directory, fileName, lines, date);

    private static int CountLinesWith(string path, string sku)
        => Step06_FileHandling.CountLinesWith_Solution(path, sku);
    // ---------------------------------------------------------------------------------

    private readonly string _directory =
        Path.Combine(Path.GetTempPath(), "pm-lecture-tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private string Given(string fileName, string content)
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, fileName);
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    // --- TASK 1 ----------------------------------------------------------------------

    [Fact]
    public void AMissingFileGivesAnEmptyList()
        => Assert.Empty(ReadStockFile(_directory, "does-not-exist.csv"));

    [Fact]
    public void ValidLinesAreParsed()
    {
        Given("stock.csv", "VEH-001;3\r\nTYR-001;64\r\n");

        var rows = ReadStockFile(_directory, "stock.csv");

        Assert.Equal(2, rows.Count);
        Assert.Equal(new StockLine("VEH-001", 3), rows[0]);
        Assert.Equal(new StockLine("TYR-001", 64), rows[1]);
    }

    [Fact]
    public void CommentsAndBlankLinesAreSkipped()
    {
        Given("stock.csv", "# stock report\r\n\r\nVEH-001;3\r\n   # indented comment\r\n");

        var rows = ReadStockFile(_directory, "stock.csv");

        Assert.Single(rows);
        Assert.Equal("VEH-001", rows[0].Sku);
    }

    [Fact]
    public void MalformedLinesAreSkipped()
    {
        Given("stock.csv", "VEH-001;3\r\nbroken line\r\nTYR-001;abc\r\nA;1;2\r\nENG-001;0\r\n");

        var rows = ReadStockFile(_directory, "stock.csv");

        Assert.Equal(2, rows.Count);
        Assert.Equal("VEH-001", rows[0].Sku);
        Assert.Equal("ENG-001", rows[1].Sku);
        Assert.Equal(0, rows[1].Quantity);      // zero is a valid quantity
    }

    [Fact]
    public void TheSkuIsTrimmedAndUppercased()
    {
        Given("stock.csv", "  veh-001  ; 3 \r\n");

        var rows = ReadStockFile(_directory, "stock.csv");

        Assert.Equal("VEH-001", rows[0].Sku);
        Assert.Equal(3, rows[0].Quantity);
    }

    [Fact]
    public void AccentedTextSurvivesTheRoundTrip()
    {
        Given("stock.csv", "TÓTH-001;1\r\n");

        var rows = ReadStockFile(_directory, "stock.csv");

        Assert.Equal("TÓTH-001", rows[0].Sku);
    }

    // --- TASK 2 ----------------------------------------------------------------------

    [Fact]
    public void WriteCreatesTheDirectoryAndReturnsThePath()
    {
        var nested = Path.Combine(_directory, "reports", "2026");

        var path = WriteStockFile(nested, "stock.csv", Array.Empty<StockLine>(), new DateOnly(2026, 3, 15));

        Assert.True(File.Exists(path));
        Assert.Equal(Path.Combine(nested, "stock.csv"), path);
    }

    [Fact]
    public void WriteStartsWithTheHeaderLine()
    {
        var lines = new[] { new StockLine("VEH-001", 3) };

        var path = WriteStockFile(_directory, "stock.csv", lines, new DateOnly(2026, 3, 15));
        var written = File.ReadAllLines(path, Encoding.UTF8);

        Assert.Equal("# stock report 2026-03-15", written[0]);
        Assert.Equal("VEH-001;3", written[1]);
    }

    [Fact]
    public void WriteOverwritesAnExistingFile()
    {
        Given("stock.csv", "OLD;999\r\n");

        var path = WriteStockFile(
            _directory, "stock.csv", new[] { new StockLine("NEW-001", 1) }, new DateOnly(2026, 3, 15));

        Assert.DoesNotContain("OLD", File.ReadAllText(path, Encoding.UTF8), StringComparison.Ordinal);
    }

    [Fact]
    public void WhatIsWrittenCanBeReadBack()
    {
        var original = new[] { new StockLine("VEH-001", 3), new StockLine("TYR-001", 64) };

        WriteStockFile(_directory, "roundtrip.csv", original, new DateOnly(2026, 3, 15));
        var rows = ReadStockFile(_directory, "roundtrip.csv");

        Assert.Equal(original, rows);
    }

    // --- TASK 3 ----------------------------------------------------------------------

    [Fact]
    public void CountLinesWithIsCaseInsensitive()
    {
        var path = Given("stock.csv", "VEH-001;3\r\nveh-001;4\r\nTYR-001;64\r\n");

        Assert.Equal(2, CountLinesWith(path, "veh-001"));
        Assert.Equal(1, CountLinesWith(path, "TYR"));
        Assert.Equal(0, CountLinesWith(path, "NOPE"));
    }

    [Fact]
    public void TheFileIsClosedAfterwards()
    {
        var path = Given("stock.csv", "VEH-001;3\r\n");

        CountLinesWith(path, "VEH");

        // Without the `using` the handle would still be open and this would throw.
        File.Delete(path);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void TheStreamingAndTheSimpleVersionAgree()
    {
        var path = Given("stock.csv", "VEH-001;3\r\nveh-002;4\r\nTYR-001;64\r\n");

        Assert.Equal(
            Step06_FileHandling.CountLinesWith_Solution(path, "VEH"),
            Step06_FileHandling.CountLinesWith_Simple(path, "VEH"));
    }
}
