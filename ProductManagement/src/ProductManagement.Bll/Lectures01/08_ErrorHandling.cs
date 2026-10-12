namespace ProductManagement.Bll.Lectures01;

// === TYPES USED BY THE TASKS =========================================================

/// <summary>A parsed price row.</summary>
public sealed record ParsedPrice(string Sku, decimal Price);

/// <summary>One failed row of an import.</summary>
public sealed record ImportFailure(int RowNumber, string Message);

/// <summary>Outcome of an import batch.</summary>
public sealed record ImportResult(int Succeeded, IReadOnlyList<ImportFailure> Failures)
{
    public int Failed => Failures.Count;
}

/// <summary>The row could not be parsed at all.</summary>
public sealed class InvalidRowException : Exception
{
    public InvalidRowException(string message)
        : base(message)
    {
    }
}

/// <summary>The product is not in the catalogue. It carries the SKU, not just a message -
/// that is the point of a custom exception: the catch block can react without parsing text.</summary>
public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException(string sku)
        : base($"Product '{sku}' was not found.")
        => Sku = sku;

    public string Sku { get; }
}

/// <summary>Stand-in for something that holds a handle.</summary>
public sealed class FakeResource : IDisposable
{
    public void Close() => Dispose();

    public void Dispose()
    {
    }
}

// =====================================================================================

/// <summary>
/// STEP 8 — Exceptions: try/catch/finally, custom exceptions, what NOT to swallow.
/// </summary>
/// <remarks>
/// Suggested time: 45-55 minutes. This matters more than it looks: the client asked for
/// consistent API error handling ("no error + 200 OK") and for proper diagnostics.
///
/// <list type="bullet">
/// <item>An exception is for the <b>unexpected</b>. Bad input from a file is expected:
///       return a result. A database that is down is unexpected: throw.</item>
/// <item><c>catch (Exception) { }</c> is how a system loses data silently.</item>
/// <item><c>throw;</c> keeps the stack trace; <c>throw ex;</c> resets it and the log then
///       points at the wrong line.</item>
/// <item><c>finally</c> always runs - but <c>using</c> is usually what you want.</item>
/// </list>
///
/// Harbour's equivalent is one broad net:
/// <code>
///     BEGIN SEQUENCE WITH { | oErr | Break( oErr ) }
///        ProcessRow( aRow )
///     RECOVER USING oError
///        LogError( oError:description )     // every failure looks the same
///     END SEQUENCE
/// </code>
/// C# lets you catch BY TYPE, which is what makes "handle this one, let that one through"
/// possible at all.
/// </remarks>
public static class Step08_ErrorHandling
{
    // ---------------------------------------------------------------------------------
    // TASK 1 - A batch import that survives bad rows
    //
    // For each row call `parse(row)` then `apply(parsed)`. They may throw:
    //   InvalidRowException      -> record it, carry on
    //   ProductNotFoundException -> record it, carry on
    //   anything else            -> let it abort the batch
    //
    // Failures keep the 1-based row number and a message.
    // Do NOT write `catch (Exception)` anywhere in this method.
    // ---------------------------------------------------------------------------------

    /// <summary>Imports a batch, surviving the rows it can.</summary>
    public static ImportResult ImportBatch(
        IReadOnlyList<string> rows,
        Func<string, ParsedPrice> parse,
        Action<ParsedPrice> apply)
    {
        // TODO 1: a counter and a List<ImportFailure>, declared BEFORE the loop
        // TODO 2: a for loop so you have the index; rowNumber is i + 1
        // TODO 3: try { parse, apply, counter++ }
        // TODO 4: catch the two exception types SEPARATELY, with different messages
        // TODO 5: return the ImportResult
        throw new NotImplementedException("Task 1");
    }

    #region Solution 1

    public static ImportResult ImportBatch_Solution(
        IReadOnlyList<string> rows,
        Func<string, ParsedPrice> parse,
        Action<ParsedPrice> apply)
    {
        var succeeded = 0;
        var failures = new List<ImportFailure>();

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNumber = i + 1;

            try
            {
                var parsed = parse(rows[i]);
                apply(parsed);
                succeeded++;
            }
            catch (InvalidRowException ex)
            {
                failures.Add(new ImportFailure(rowNumber, $"Malformed row: {ex.Message}"));
            }
            catch (ProductNotFoundException ex)
            {
                // The exception CARRIES the sku, so we do not have to parse the message.
                failures.Add(new ImportFailure(rowNumber, $"Unknown product: {ex.Sku}"));
            }
        }

        return new ImportResult(succeeded, failures);
    }

    // - There is no `catch (Exception)`. A NullReferenceException or a SqlException
    //   propagates, which is right: those mean the code or the environment is broken,
    //   not the data.
    // - "Record and continue" vs "abort" is a BUSINESS decision. Ask which TL processes
    //   are nightly batches and which are interactive saves.
    // - What would a blanket catch cost here? A typo producing a NullReferenceException
    //   would be reported as "row N failed" ten thousand times, and the import would
    //   finish "successfully" having applied nothing.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 2 - Throw or return?
    //
    //   GetRequired(sku) -> returns the product, throws when it is not there
    //   TryGet(sku, out) -> returns false, never throws
    //
    // A null or blank sku is a PROGRAMMING error, not a missing product: GetRequired must
    // throw ArgumentException for it, not ProductNotFoundException. That difference is
    // the real content of this task.
    // ---------------------------------------------------------------------------------

    /// <summary>Returns the product or throws.</summary>
    public static ParsedPrice GetRequired(IReadOnlyDictionary<string, ParsedPrice> catalog, string sku)
    {
        // TODO 1: blank sku -> ArgumentException (the CALLER made a mistake)
        // TODO 2: TryGetValue -> ProductNotFoundException when absent (the DATA is missing)
        throw new NotImplementedException("Task 2a");
    }

    /// <summary>Returns the product, or false. Never throws for a missing product.</summary>
    public static bool TryGet(
        IReadOnlyDictionary<string, ParsedPrice> catalog,
        string? sku,
        out ParsedPrice? product)
    {
        // TODO 1: assign `product` first
        // TODO 2: blank sku -> false
        // TODO 3: TryGetValue
        throw new NotImplementedException("Task 2b");
    }

    #region Solution 2

    public static ParsedPrice GetRequired_Solution(
        IReadOnlyDictionary<string, ParsedPrice> catalog,
        string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("Sku is required.", nameof(sku));
        }

        if (!catalog.TryGetValue(sku.Trim().ToUpperInvariant(), out var product))
        {
            throw new ProductNotFoundException(sku);
        }

        return product;
    }

    public static bool TryGet_Solution(
        IReadOnlyDictionary<string, ParsedPrice> catalog,
        string? sku,
        out ParsedPrice? product)
    {
        product = null;

        if (string.IsNullOrWhiteSpace(sku))
        {
            return false;
        }

        return catalog.TryGetValue(sku.Trim().ToUpperInvariant(), out product);
    }

    // - ArgumentException says "you called me wrong, fix your code". ProductNotFound says
    //   "what you asked for is not here, which is normal". A web API maps the first to 500
    //   and the second to 404. Conflating them is why so many systems return 500 for
    //   things that are not server errors.
    // - Both patterns exist in the BCL for this reason: int.Parse / int.TryParse. Offering
    //   both is a legitimate design - the caller knows whether "missing" is normal.
    // - Exceptions are not free. In a loop over 100 000 rows, prefer TryGet.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 3 - Four broken catch blocks. Say what is lost, then fix it.
    // ---------------------------------------------------------------------------------

    /// <summary>Bug 1.</summary>
    public static decimal ParsePriceBug1(string text)
    {
        try
        {
            return decimal.Parse(text);
        }
        catch (Exception)
        {
            return 0m;
        }
    }

    /// <summary>Bug 2.</summary>
    public static void ApplyBug2(Action work)
    {
        try
        {
            work();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong");

            // The built-in analyser flags this as CA2200 before you even run it.
            // Suppressed only so the project builds clean - in real code, FIX it.
#pragma warning disable CA2200
            throw ex;
#pragma warning restore CA2200
        }
    }

    /// <summary>Bug 3.</summary>
    public static string ReadBug3(Func<string> read)
    {
        var resource = new FakeResource();

        try
        {
            return read();
        }
        finally
        {
            resource.Close();
        }
    }

    /// <summary>Bug 4.</summary>
    public static void SaveBug4(Action save, Action<string> log)
    {
        try
        {
            save();
        }
        catch (Exception ex)
        {
            log($"Error: {ex.Message}");
        }
    }

    #region Solution 3

    // Bug 1 - swallows everything and returns a plausible zero. A null input, an
    // OutOfMemoryException and a malformed number all become 0.00 on an invoice. Parse
    // also uses the CURRENT culture, so "1.5" is 15 on a Hungarian machine. This is
    // Harbour's Val() behaviour, reinvented by hand.
    public static bool TryParsePrice_Solution(string? text, out decimal price)
        => decimal.TryParse(
            text,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out price);

    // If the caller wants a default, let THEM choose it:
    //     var price = TryParsePrice(text, out var p) ? p : 0m;

    // Bug 2 - `throw ex;` resets the stack trace to this line, so the production log
    // points at ApplyBug2, which is never where the bug is.
    public static void Apply_Solution(Action work, Action<string, Exception> log)
    {
        try
        {
            work();
        }
        catch (Exception ex)
        {
            log("Work failed", ex);   // the exception OBJECT, not just its message
            throw;                    // bare throw - stack trace preserved
        }
    }

    // Bug 3 - finally is correct but manual. `using` cannot be forgotten, and it also
    // covers the case where the constructor itself throws.
    public static string Read_Solution(Func<string> read)
    {
        using var resource = new FakeResource();
        return read();
    }

    // Bug 4 - swallows every unexpected exception and reports it as handled. The caller is
    // told nothing, and save() may have done half its work.
    public static void Save_Solution(Action save, Action<string> log)
    {
        try
        {
            save();
        }
        catch (ProductNotFoundException ex)
        {
            log($"Not saved, unknown product: {ex.Sku}");
        }

        // Everything else propagates; the API layer turns it into a 500 and a logged
        // stack trace - the correct outcome for "we do not know what happened".
    }

    // The rule: catch only what you can actually do something about; let everything else
    // go up. And an exception must never become a 200 OK with an informational message.

    #endregion
}
