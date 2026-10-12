namespace ProductManagement.Bll.Lectures01;

// === TYPES USED BY THE TASKS =========================================================

/// <summary>Parameter object replacing a 6-parameter signature.</summary>
public sealed class OrderRequest
{
    public int UserId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public int Quantity { get; init; } = 1;

    public decimal UnitPrice { get; init; }

    public bool Express { get; init; }

    public string? Note { get; init; }
}

// =====================================================================================

/// <summary>
/// STEP 4 — Methods and parameter passing.
/// </summary>
/// <remarks>
/// Suggested time: 40-50 minutes. This maps directly onto "how do we parameterise the
/// several thousand procedures we have today".
///
/// <list type="bullet">
/// <item>Default values and named arguments instead of ten overloads.</item>
/// <item>Overloads are resolved at COMPILE time from the static argument types.</item>
/// <item><c>out</c> belongs to the Try-pattern; <c>ref</c> is rare.</item>
/// <item>Past about four parameters, pass an object.</item>
/// </list>
///
/// Harbour does optional parameters by checking for NIL:
/// <code>
///     FUNCTION GrossPrice( nNet, nVatPercent )
///        IF nVatPercent == NIL
///           nVatPercent := 27
///        ENDIF
///        RETURN Round( nNet * ( 1 + nVatPercent / 100 ), 2 )
/// </code>
/// C# puts the default in the signature, where the caller can see it.
/// </remarks>
public static class Step04_Methods
{
    // ---------------------------------------------------------------------------------
    // TASK 1 - Default parameters
    //
    // Gross price from net. VAT defaults to 27 percent, rounded to 2 decimals.
    // Then call it three ways and compare readability:
    //     GrossPrice(100m)   GrossPrice(100m, 5m)   GrossPrice(100m, vatPercent: 5m)
    // ---------------------------------------------------------------------------------

    /// <summary>Gross price, rounded to 2 decimals.</summary>
    public static decimal GrossPrice(decimal netPrice, decimal vatPercent = 27m)
    {
        // TODO 1: net * (1 + vat/100)
        // TODO 2: Math.Round to 2 decimals, passing MidpointRounding explicitly
        throw new NotImplementedException("Task 1");
    }

    #region Solution 1

    public static decimal GrossPrice_Solution(decimal netPrice, decimal vatPercent = 27m)
        => Math.Round(netPrice * (1 + vatPercent / 100m), 2, MidpointRounding.AwayFromZero);

    // - .NET's default rounding is ToEven, so Math.Round(2.5m) is 2. Harbour's Round()
    //   gives 3. Always pass the mode for money.
    // - A default value is compiled into the CALLER. Across a NuGet boundary, changing it
    //   does nothing until callers recompile. Inside one solution it never matters.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 2 - The Try pattern
    //
    // Split "VEH-001" into group code ("VEH") and number (1). Return false instead of
    // throwing when it does not match. This is the shape of int.TryParse.
    // ---------------------------------------------------------------------------------

    /// <summary>Tries to split a SKU into group code and number.</summary>
    public static bool TryParseSku(string? sku, out string groupCode, out int number)
    {
        // TODO 1: assign BOTH out parameters first - every path out must leave them set
        // TODO 2: null or blank -> false
        // TODO 3: Split on '-'; anything other than 2 parts -> false
        // TODO 4: int.TryParse the second part; failure -> false
        // TODO 5: set the outputs (group code uppercased) and return true
        throw new NotImplementedException("Task 2");
    }

    #region Solution 2

    public static bool TryParseSku_Solution(string? sku, out string groupCode, out int number)
    {
        groupCode = string.Empty;
        number = 0;

        if (string.IsNullOrWhiteSpace(sku))
        {
            return false;
        }

        var parts = sku.Split('-');

        if (parts.Length != 2 || !int.TryParse(parts[1], out var parsed))
        {
            return false;
        }

        groupCode = parts[0].ToUpperInvariant();
        number = parsed;
        return true;
    }

    // - The compiler forces every `out` to be assigned on EVERY path, failures included.
    // - At the call site: `if (TryParseSku(input, out var code, out var num))`.
    //   `out _` discards one you do not need.
    // - Harbour returns several values by passing an array or by @-reference; the Try
    //   pattern is the C# idiom and the editor shows it in the signature.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 3 - From six parameters to one object
    //
    // Nobody can read CreateOrder(1, "VEH-001", 2, 18900m, true, null) at a glance.
    //
    // Summarise(OrderRequest) returns:
    //     "user 1: VEH-001 x2 = 37800.00 + express 4900.00 = 42700.00"
    // The express part (and the trailing total) is omitted when Express is false.
    // ---------------------------------------------------------------------------------

    /// <summary>One-line summary of an order request.</summary>
    public static string Summarise(OrderRequest request)
    {
        // TODO 1: line total = Quantity * UnitPrice, rounded to 2 decimals
        // TODO 2: "user {id}: {sku} x{qty} = {total}" - the F2 format is $"{value:F2}"
        // TODO 3: not Express -> return it as it is
        // TODO 4: otherwise append " + express 4900.00 = {grand total}"
        throw new NotImplementedException("Task 3");
    }

    #region Solution 3

    private const decimal ExpressFee = 4_900m;

    public static string Summarise_Solution(OrderRequest request)
    {
        var lineTotal = Math.Round(request.Quantity * request.UnitPrice, 2, MidpointRounding.AwayFromZero);
        var text = $"user {request.UserId}: {request.Sku} x{request.Quantity} = {lineTotal:F2}";

        if (!request.Express)
        {
            return text;
        }

        return text + $" + express {ExpressFee:F2} = {lineTotal + ExpressFee:F2}";
    }

    // The parameter object: the call site reads like a form, adding a field does not break
    // every caller, there is one place for validation, and it is the same object you will
    // later bind from JSON in the API.

    #endregion

    // ---------------------------------------------------------------------------------
    // TASK 4 - Overload puzzle (10 minutes, discussion)
    //
    // Predict which overload each call picks, then check:
    //     Apply(100m);   Apply(100m, 10);   Apply(100m, 10m);   Apply(100, 10);
    //
    // Then express the same API with ONE method plus a default value.
    // ---------------------------------------------------------------------------------

    /// <summary>Overload A.</summary>
    public static decimal Apply(decimal amount) => amount;

    /// <summary>Overload B.</summary>
    public static decimal Apply(decimal amount, int percent) => amount * (100 - percent) / 100m;

    /// <summary>Overload C.</summary>
    public static decimal Apply(decimal amount, decimal percent) => amount * (100m - percent) / 100m;

    #region Solution 4

    // Apply(100m)      -> A, only one candidate takes one argument
    // Apply(100m, 10)  -> B, `10` is an int literal and an exact match beats a conversion
    // Apply(100m, 10m) -> C, exact match again
    // Apply(100, 10)   -> B, `100` converts to decimal, `10` matches int exactly
    //
    // Deterministic, but not obvious to the next reader. Two overloads differing only by
    // a numeric type are a maintenance trap.

    /// <summary>The version that should have been written in the first place.</summary>
    public static decimal Apply_Solution(decimal amount, decimal percent = 0m)
        => amount * (100m - percent) / 100m;

    // Overloads earn their keep when the parameter LISTS genuinely differ -
    // Find(int id) vs Find(string sku) - not to simulate optional parameters.

    #endregion
}
