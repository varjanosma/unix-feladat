using System.Globalization;

namespace ProductManagement.Bll.Lectures01;

/// <summary>
/// STEP 1 — Built-in data types: numbers, text, dates and times.
/// </summary>
/// <remarks>
/// Suggested time: 50-60 minutes. More walkthrough than lab: step through the examples in
/// the debugger and let the watch window do the talking. Three short tasks at the end.
///
/// Harbour has four types - numeric, character, date, logical - and the runtime works the
/// rest out. Here each of those splits into several, and picking the wrong one is a bug.
/// The two that matter most: money is <c>decimal</c>, and a point in time is
/// <c>DateTimeOffset</c>.
/// </remarks>
public static class Step01_BuiltInTypes
{
    // =================================================================================
    // 1 - WHOLE NUMBERS
    //
    //   byte 0..255 | short ±32k | int ±2.1 billion | long ±9.2*10^18
    //
    // Use int unless you have a reason. long for ids that may pass 2 billion, for epoch
    // milliseconds and for file sizes.
    //
    // Harbour:
    //     nValue := 7 / 2        // 3.5 - one numeric type, always a fraction
    // =================================================================================

    /// <summary>Integer division truncates - it does not round.</summary>
    public static (int Quotient, int Remainder, double Exact) IntegerDivision()
    {
        var quotient = 7 / 2;      // 3, not 3.5 and not 4
        var remainder = 7 % 2;     // 1
        var exact = 7 / 2.0;       // 3.5 - one double operand makes the whole expression double

        return (quotient, remainder, exact);
    }

    /// <summary>Crossing the limits.</summary>
    public static (int Max, int WrappedAround, long Safe) Overflow()
    {
        var max = int.MaxValue;                      // 2_147_483_647
        var wrapped = unchecked(int.MaxValue + 1);   // -2_147_483_648, no exception
        var safe = (long)int.MaxValue + 1;           // 2_147_483_648

        return (max, wrapped, safe);
    }

    // =================================================================================
    // 2 - FRACTIONS: double vs decimal
    //
    //   double   base 2, ~15 digits, fast        - measurements, science
    //   decimal  base 10, 28 digits, slower      - money, and only money
    //
    // 0.1 has no exact base-2 representation, exactly as 1/3 has none in base 10.
    // =================================================================================

    /// <summary>The demo that lands every time. Run it.</summary>
    public static (bool DoubleIsExact, bool DecimalIsExact, string DoubleSum) MoneyPrecision()
    {
        var doubleSum = 0.1 + 0.2;       // 0.30000000000000004
        var decimalSum = 0.1m + 0.2m;    // 0.3 exactly

        return (
            doubleSum == 0.3,
            decimalSum == 0.3m,
            doubleSum.ToString("R", CultureInfo.InvariantCulture));
    }

    // Inside a loop over 100 000 invoice lines the error accumulates, and the total
    // disagrees with the sum of the printed lines. Every accountant notices.
    // The `m` suffix is not optional: `decimal price = 18900.50;` does not compile.

    /// <summary>Rounding is not what you expect by default.</summary>
    public static (decimal Bankers, decimal Commercial, decimal Money) Rounding()
    {
        var bankers = Math.Round(2.5m);                                   // 2 - "banker's rounding"
        var commercial = Math.Round(2.5m, MidpointRounding.AwayFromZero);  // 3
        var money = Math.Round(18_900.555m, 2, MidpointRounding.AwayFromZero);

        return (bankers, commercial, money);
    }

    // Harbour's Round( 2.5, 0 ) gives 3. .NET defaults to the other rule, so ALWAYS pass
    // MidpointRounding for money.

    // =================================================================================
    // 3 - bool and char
    //
    //   bool  true/false only. `if (count)` does not compile - and `if (x = 5)` cannot
    //         silently assign either.
    //   char  one UTF-16 unit in single quotes: 'a'. Not a one-character string.
    // =================================================================================

    /// <summary>bool and char basics.</summary>
    public static (bool Parsed, char First, int Code, string Repeated) BoolAndChar()
    {
        var parsed = bool.TryParse("true", out var flag) && flag;
        var first = "VEH-001"[0];           // 'V' - indexing a string gives a char
        var code = first;                   // 86 - implicit char -> int
        var repeated = new string('-', 5);  // "-----"

        return (parsed, first, code, repeated);
    }

    // =================================================================================
    // 4 - DATES AND TIMES
    //
    //   DateTime        date + time + a Kind flag (Utc / Local / Unspecified)
    //   DateTimeOffset  date + time + the real offset from UTC     <- prefer this
    //   DateOnly        a calendar date, no time                   (.NET 6+)
    //   TimeOnly        a time of day, no date                     (.NET 6+)
    //   TimeSpan        a DURATION, not a point in time
    //
    // Harbour has one date type and no time zone at all:
    //     dToday   := Date()
    //     dDue     := dToday + 30        // date arithmetic in days
    //     nDays    := dDue - dToday      // difference is a number
    //     cStored  := DToS( dDue )       // "20260414"
    // =================================================================================

    /// <summary>DateTime carries a Kind flag, and it is easy to lose.</summary>
    public static (DateTimeKind UtcKind, DateTimeKind ParsedKind, bool EqualAcrossKinds) TheKindTrap()
    {
        var utc = DateTime.UtcNow;

        // Parsed and database-loaded values come back as Unspecified - "nobody knows".
        var parsed = DateTime.Parse("2026-03-15T10:30:00", CultureInfo.InvariantCulture);

        // Equality ignores Kind, so these are "equal" while meaning different moments:
        var equal = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc)
                 == new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Local);

        return (utc.Kind, parsed.Kind, equal);
    }

    /// <summary>DateTimeOffset cannot lose the offset.</summary>
    public static (DateTimeOffset Utc, TimeSpan Offset, bool SameInstant) OffsetIsSafer()
    {
        var local = new DateTimeOffset(2026, 3, 15, 11, 30, 0, TimeSpan.FromHours(1));
        var utc = local.ToUniversalTime();   // 2026-03-15T10:30:00+00:00

        return (utc, local.Offset, local == utc);   // same instant, so true
    }

    /// <summary>A date that is only a date.</summary>
    public static (DateOnly Invoice, DateOnly DueDate, int DaysLeft, TimeOnly Opening) DatesWithoutTime()
    {
        var invoice = new DateOnly(2026, 3, 15);
        var dueDate = invoice.AddDays(30);
        var today = new DateOnly(2026, 3, 20);
        var daysLeft = dueDate.DayNumber - today.DayNumber;

        return (invoice, dueDate, daysLeft, new TimeOnly(8, 30));
    }

    /// <summary>TimeSpan is a duration.</summary>
    public static (TimeSpan Age, double TotalHours, int HoursComponent, string Formatted) Durations()
    {
        var placed = new DateTimeOffset(2026, 3, 15, 10, 30, 0, TimeSpan.Zero);
        var delivered = new DateTimeOffset(2026, 3, 18, 14, 45, 0, TimeSpan.Zero);

        var age = delivered - placed;

        return (age, age.TotalHours, age.Hours, $"{age.Days} days {age.Hours} hours");
    }

    // TotalHours (76.25) vs Hours (4) is a classic: the first is the whole duration in
    // hours, the second is only the hour component of "3 days 4 hours".

    /// <summary>Formatting and parsing, always with an explicit culture.</summary>
    public static (string Iso, string Hungarian, bool Parsed, DateOnly Value) FormattingDates()
    {
        var date = new DateOnly(2026, 3, 15);

        var iso = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var hungarian = date.ToString("d", new CultureInfo("hu-HU"));

        var parsed = DateOnly.TryParseExact(
            "2026-03-15", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value);

        return (iso, hungarian, parsed, value);
    }

    // DateTime.Parse(text) with no culture uses the machine setting, so "03/04/2026" is
    // 3 April on one server and 4 March on another. Machine data -> InvariantCulture.

    // =================================================================================
    // 5 - Guid and enum
    // =================================================================================

    /// <summary>An enum is a named set of integers. Always write the numbers out.</summary>
    public static (OrderStatus Parsed, string Name, int Number, bool IsDefined) Enums()
    {
        var status = OrderStatus.Shipped;

        var parsed = Enum.TryParse<OrderStatus>("Cancelled", ignoreCase: true, out var value)
            ? value
            : OrderStatus.New;

        return (parsed, status.ToString(), (int)status, Enum.IsDefined((OrderStatus)99));
    }

    // `(OrderStatus)99` compiles and runs - Enum.IsDefined is the guard. And without
    // explicit numbers, inserting a value in the middle renumbers everything after it,
    // silently changing the meaning of every number already in the database.

    // =================================================================================
    // 6 - CONVERSIONS
    //
    // Harbour: Val( "42" ) returns 0 for garbage, which is why bad input hides for years.
    // =================================================================================

    /// <summary>Widening, truncating, and the three ways to read a number from text.</summary>
    public static (long Widened, int Truncated, int Parsed, bool Tried) Conversions()
    {
        int small = 42;
        long widened = small;            // implicit: nothing can be lost
        var truncated = (int)3.99;       // explicit cast: 3 - truncates, never rounds

        var parsed = int.Parse("42", CultureInfo.InvariantCulture);   // throws on bad input
        var tried = int.TryParse("abc", out _);                       // false, no exception

        return (widened, truncated, parsed, tried);
    }

    // Parse when bad input is a BUG. TryParse when bad input is EXPECTED - files, forms,
    // query strings. That is 90% of import and API code.

    // =================================================================================
    // TASK 1 - Warranty expiry
    //
    // Given the purchase date and the warranty length in months, return the expiry date
    // and whether it is still valid on `today`.
    //   - 0 months means no warranty at all: never valid
    //   - `today` is a parameter. Ask the class why, before they start writing.
    // =================================================================================

    /// <summary>Warranty expiry date and whether it is still valid.</summary>
    public static (DateOnly ExpiresOn, bool IsValid) Warranty(DateOnly purchased, int warrantyMonths, DateOnly today)
    {
        // TODO 1: expiry = purchased.AddMonths(warrantyMonths)
        // TODO 2: valid while today <= expiry (DateOnly supports <= directly)
        // TODO 3: handle warrantyMonths == 0 explicitly
        throw new NotImplementedException("Task 1");
    }

    #region Solution 1

    public static (DateOnly ExpiresOn, bool IsValid) Warranty_Solution(
        DateOnly purchased,
        int warrantyMonths,
        DateOnly today)
    {
        var expiresOn = purchased.AddMonths(warrantyMonths);
        var isValid = warrantyMonths > 0 && today <= expiresOn;

        return (expiresOn, isValid);
    }

    // - AddMonths is calendar-aware: 31 January + 1 month is 28 February.
    //   Harbour's dDate + 30 is days only, and is wrong for "one month" every other month.
    // - `today` is a parameter because a method reading DateTime.Today is untestable:
    //   you cannot write a test for "expired last year" that still passes next year.

    #endregion

    // =================================================================================
    // TASK 2 - Order age, formatted
    //
    //     >= 1 day   ->  "3 days 4 hours"
    //     >= 1 hour  ->  "4 hours 15 minutes"
    //     otherwise  ->  "15 minutes"
    //     negative   ->  "not yet"
    // =================================================================================

    /// <summary>Human-readable age of an order.</summary>
    public static string OrderAge(DateTimeOffset placedAt, DateTimeOffset now)
    {
        // TODO 1: subtract the two instants - the result is a TimeSpan
        // TODO 2: negative -> "not yet"
        // TODO 3: pick the shape. You want the COMPONENTS (Days, Hours, Minutes),
        //         not TotalDays / TotalHours
        throw new NotImplementedException("Task 2");
    }

    #region Solution 2

    public static string OrderAge_Solution(DateTimeOffset placedAt, DateTimeOffset now)
    {
        var age = now - placedAt;

        if (age < TimeSpan.Zero)
        {
            return "not yet";
        }

        return age switch
        {
            { Days: > 0 } => $"{age.Days} days {age.Hours} hours",
            { Hours: > 0 } => $"{age.Hours} hours {age.Minutes} minutes",
            _ => $"{age.Minutes} minutes",
        };
    }

    // - Subtracting two DateTimeOffset values is correct across time zones and across a
    //   daylight-saving change, because both sides carry their offset.
    // - TotalHours here would print "76 hours 15 minutes".

    #endregion

    // =================================================================================
    // TASK 3 - Pick the type (discussion, 10 minutes, no code)
    //
    //   a) a unit price            d) an invoice date        g) a customer id
    //   b) a stock quantity        e) the shop opening time  h) an order status
    //   c) when an order was placed f) how long delivery took i) a weight to 3 decimals
    // =================================================================================

    #region Solution 3

    // a) decimal          b) int              c) DateTimeOffset
    // d) DateOnly         e) TimeOnly         f) TimeSpan
    // g) int              h) enum             i) decimal
    //
    // The question each time: an exact decimal quantity (decimal), a count (int), a
    // measurement where tiny error is fine (double), or a moment / date / time of day /
    // duration? Harbour collapses all of these into four types. Here the compiler makes
    // you decide, and that decision is the documentation.

    #endregion
}

/// <summary>Sample enum. Note the explicit numbers.</summary>
public enum OrderStatus
{
    New = 1,
    Paid = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
}
