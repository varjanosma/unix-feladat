using System.Globalization;
using System.Text;

namespace ProductManagement.Bll.Lectures01;

// === TYPES USED BY THE TASKS =========================================================

/// <summary>One line of a simple CSV stock file: SKU;quantity</summary>
public sealed record StockLine(string Sku, int Quantity);

// =====================================================================================

/// <summary>
/// STEP 6 — File handling, the simple version.
/// </summary>
/// <remarks>
/// Suggested time: 30-40 minutes. Deliberately shallow: enough to read and write a text
/// file properly, no streams-and-buffers deep dive.
///
/// <list type="bullet">
/// <item><c>File.ReadAllLines</c> / <c>WriteAllText</c> are the simple API, and the right
///       one for a file that fits in memory. <c>File.ReadLines</c> (no "All") streams.</item>
/// <item>Always pass the <b>encoding</b> explicitly.</item>
/// <item><c>Path.Combine</c>, never string concatenation with a backslash.</item>
/// <item><c>using</c> for anything that holds a handle.</item>
/// </list>
///
/// Harbour equivalents:
/// <code>
///     cText := MemoRead( cFile )                  // read the whole file
///     MemoWrit( cFile, cText )                    // write it back
///     aLines := hb_ATokens( cText, hb_eol() )
///     nHandle := FOpen( cFile ) ... FClose( nHandle )
/// </code>
/// MemoRead returns "" for a missing file - the same silent-default problem as Val().
///
/// Every task takes the directory as a PARAMETER. Nothing here writes to a hard-coded path.
/// </remarks>
public static class Step06_FileHandling
{
    // ---------------------------------------------------------------------------------
    // TASK 1 - Read a stock file
    //
    //     VEH-001;3
    //     # comment lines start with a hash
    //     TYR-001;64
    //
    //   - missing file        -> empty list, do not throw
    //   - blank and # lines   -> skipped
    //   - not "text;number"   -> skipped (reporting the errors is step 8)
    //   - the sku is trimmed and uppercased
    // ---------------------------------------------------------------------------------

    /// <summary>Reads a semicolon-separated stock file.</summary>
    public static List<StockLine> ReadStockFile(string directory, string fileName)
    {
        // TODO 1: Path.Combine(directory, fileName)
        // TODO 2: !File.Exists -> return an empty list
        // TODO 3: File.ReadAllLines(path, Encoding.UTF8)
        // TODO 4: skip blank lines and lines starting with '#'
        // TODO 5: Split on ';'; skip anything that is not exactly 2 parts
        // TODO 6: int.TryParse the quantity; skip on failure
        // TODO 7: add a StockLine with the trimmed, uppercased sku
        throw new NotImplementedException("Task 1");
    }

    #region Solution 1

    public static List<StockLine> ReadStockFile_Solution(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        var result = new List<StockLine>();

        if (!File.Exists(path))
        {
            return result;
        }

        foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split(';');

            if (parts.Length != 2)
            {
                continue;
            }

            if (!int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity))
            {
                continue;
            }

            result.Add(new StockLine(parts[0].Trim().ToUpperInvariant(), quantity));
        }

        return result;
    }

    // - Path.Combine handles the separator and an already-absolute second part.
    //   `directory + "\\" + fileName` does neither and breaks on Linux, which a .NET 6
    //   app can run on.
    // - Encoding.UTF8 explicitly. Hungarian text written by an older Windows tool is often
    //   Windows-1250, and reading it as UTF-8 gives mojibake instead of an error. If you
    //   see "Ã¡" where "á" should be, this is why.
    // - ReadAllLines loads the whole file; ReadLines streams. One word, very different
    //   memory profile.
    // - Returning an empty list for a missing file is a DECISION, not a default.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 2 - Write a report file
    //
    //     # stock report 2026-03-15
    //     VEH-001;3
    //
    //   - create the directory if needed, overwrite an existing file
    //   - UTF-8, date formatted with InvariantCulture
    //   - return the full path
    // ---------------------------------------------------------------------------------

    /// <summary>Writes a stock report and returns the path.</summary>
    public static string WriteStockFile(string directory, string fileName, IEnumerable<StockLine> lines, DateOnly date)
    {
        // TODO 1: Directory.CreateDirectory(directory) - a no-op when it exists
        // TODO 2: Path.Combine
        // TODO 3: a StringBuilder: the header line, then the rows
        // TODO 4: File.WriteAllText(path, content, Encoding.UTF8)
        // TODO 5: return the path
        throw new NotImplementedException("Task 2");
    }

    #region Solution 2

    public static string WriteStockFile_Solution(
        string directory,
        string fileName,
        IEnumerable<StockLine> lines,
        DateOnly date)
    {
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, fileName);
        var builder = new StringBuilder();

        builder.AppendLine($"# stock report {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

        foreach (var line in lines)
        {
            builder.AppendLine($"{line.Sku};{line.Quantity}");
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);

        return path;
    }

    // - CreateDirectory does nothing if the directory exists; no Exists check needed and
    //   no race condition.
    // - WriteAllText OVERWRITES silently. To avoid leaving a half-written file behind if
    //   the process dies, write to a temporary name and File.Move over the target.
    // - Encoding.UTF8 writes a BOM; `new UTF8Encoding(false)` does not. Some partner
    //   parsers choke on the BOM - the single most common "works on my machine" file bug.
    // - AppendLine uses Environment.NewLine (\r\n on Windows, \n on Linux). For a file
    //   another system reads, pin it explicitly.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 3 - Read a large file line by line
    //
    // Count the lines containing a SKU WITHOUT loading the file into memory.
    // Then: what happens if you forget the `using`? Try opening the file twice.
    // ---------------------------------------------------------------------------------

    /// <summary>Counts the lines containing the SKU, streaming the file.</summary>
    public static int CountLinesWith(string path, string sku)
    {
        // TODO 1: using var reader = new StreamReader(path, Encoding.UTF8);
        // TODO 2: while reader.ReadLine() returns a non-null line
        // TODO 3: count the lines containing the sku, case-insensitively
        throw new NotImplementedException("Task 3");
    }

    #region Solution 3

    public static int CountLinesWith_Solution(string path, string sku)
    {
        using var reader = new StreamReader(path, Encoding.UTF8);

        var count = 0;
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Contains(sku, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The same thing with the simple API, which also streams.</summary>
    public static int CountLinesWith_Simple(string path, string sku)
        => File.ReadLines(path).Count(line => line.Contains(sku, StringComparison.OrdinalIgnoreCase));

    // - `using var` closes the handle at the end of the method. Without it the handle stays
    //   open until the GC gets round to it, and the next open fails with
    //   "the process cannot access the file because it is being used by another process".
    //   This is the C# answer to forgetting FClose().
    // - `string.Contains(value, StringComparison)` - the single-argument overload is
    //   ordinal and case-sensitive, and people assume otherwise.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 4 - What can go wrong? (discussion, 10 minutes, no code)
    //
    //   a) the file does not exist      d) the file is open in Excel
    //   b) the directory does not exist e) the disk is full
    //   c) no permission to read        f) the path is too long
    //
    // For each: should the method handle it, or let it propagate?
    // ---------------------------------------------------------------------------------

    #region Solution 4

    // a) FileNotFoundException      - we HANDLED it. Right only if "no file" is normal.
    // b) DirectoryNotFoundException - a DIFFERENT exception from (a); catching only
    //                                 FileNotFoundException misses it.
    // c) UnauthorizedAccessException - propagate. Swallowing it means the nightly import
    //                                 silently does nothing for a week.
    // d) IOException ("used by another process") - propagate, or retry if this is known to
    //                                 happen often. Very common with files humans touch.
    // e) IOException                - propagate.
    // f) PathTooLongException       - propagate; a configuration problem.
    //
    // The rule, which comes back in step 8: catch only what you can actually do something
    // about. A blanket `catch (Exception) { return new List<StockLine>(); }` turns every
    // one of these into "the file was empty", which is how an import loses a day's data.

    #endregion
}
