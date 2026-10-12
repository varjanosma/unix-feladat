using ProductManagement.Bll.Lectures01;
using Xunit;

namespace ProductManagement.Bll.Tests;

/// <summary>
/// Tests for STEP 1. The first group checks the claims the lecture makes about the
/// built-in types; the rest check the tasks.
/// </summary>
public class Step01_BuiltInTypesTests
{
    // --- SWITCH HERE -----------------------------------------------------------------
    // These point at the reference solutions, so the suite is green out of the box.
    // Drop the "_Solution" to run the tests against your own implementation.
    private static (DateOnly ExpiresOn, bool IsValid) Warranty(DateOnly purchased, int months, DateOnly today)
        => Step01_BuiltInTypes.Warranty_Solution(purchased, months, today);

    private static string OrderAge(DateTimeOffset placedAt, DateTimeOffset now)
        => Step01_BuiltInTypes.OrderAge_Solution(placedAt, now);
    // ---------------------------------------------------------------------------------

    [Fact]
    public void IntegerDivisionTruncates()
    {
        var (quotient, remainder, exact) = Step01_BuiltInTypes.IntegerDivision();

        Assert.Equal(3, quotient);      // not 3.5, not 4
        Assert.Equal(1, remainder);
        Assert.Equal(3.5, exact);
    }

    [Fact]
    public void IntOverflowWrapsAroundSilently()
    {
        var (max, wrapped, safe) = Step01_BuiltInTypes.Overflow();

        Assert.Equal(2_147_483_647, max);
        Assert.Equal(-2_147_483_648, wrapped);
        Assert.Equal(2_147_483_648L, safe);
    }

    [Fact]
    public void DoubleCannotRepresentMoney()
    {
        var (doubleIsExact, decimalIsExact, doubleSum) = Step01_BuiltInTypes.MoneyPrecision();

        Assert.False(doubleIsExact);
        Assert.True(decimalIsExact);
        Assert.Equal("0.30000000000000004", doubleSum);
    }

    [Fact]
    public void DefaultRoundingIsBankers()
    {
        var (bankers, commercial, money) = Step01_BuiltInTypes.Rounding();

        Assert.Equal(2m, bankers);       // the surprise
        Assert.Equal(3m, commercial);
        Assert.Equal(18_900.56m, money);
    }

    [Fact]
    public void DateTimeEqualityIgnoresKind()
    {
        var (utcKind, parsedKind, equalAcrossKinds) = Step01_BuiltInTypes.TheKindTrap();

        Assert.Equal(DateTimeKind.Utc, utcKind);
        Assert.Equal(DateTimeKind.Unspecified, parsedKind);
        Assert.True(equalAcrossKinds);   // same value, different meaning
    }

    [Fact]
    public void DateTimeOffsetKeepsTheInstant()
    {
        var (utc, offset, sameInstant) = Step01_BuiltInTypes.OffsetIsSafer();

        Assert.Equal(new DateTimeOffset(2026, 3, 15, 10, 30, 0, TimeSpan.Zero), utc);
        Assert.Equal(TimeSpan.FromHours(1), offset);
        Assert.True(sameInstant);
    }

    [Fact]
    public void DateOnlyArithmetic()
    {
        var (invoice, dueDate, daysLeft, opening) = Step01_BuiltInTypes.DatesWithoutTime();

        Assert.Equal(new DateOnly(2026, 3, 15), invoice);
        Assert.Equal(new DateOnly(2026, 4, 14), dueDate);
        Assert.Equal(25, daysLeft);
        Assert.Equal(new TimeOnly(8, 30), opening);
    }

    [Fact]
    public void TotalHoursIsNotHours()
    {
        var (age, totalHours, hoursComponent, formatted) = Step01_BuiltInTypes.Durations();

        Assert.Equal(TimeSpan.FromHours(76.25), age);
        Assert.Equal(76.25, totalHours);
        Assert.Equal(4, hoursComponent);          // the component, not the total
        Assert.Equal("3 days 4 hours", formatted);
    }

    [Fact]
    public void CultureChangesTheOutput()
    {
        var (iso, hungarian, parsed, value) = Step01_BuiltInTypes.FormattingDates();

        Assert.Equal("2026-03-15", iso);
        Assert.NotEqual(iso, hungarian);
        Assert.True(parsed);
        Assert.Equal(new DateOnly(2026, 3, 15), value);
    }

    [Fact]
    public void EnumCanHoldAnUndefinedValue()
    {
        var (parsed, name, number, isDefined) = Step01_BuiltInTypes.Enums();

        Assert.Equal(OrderStatus.Cancelled, parsed);
        Assert.Equal("Shipped", name);
        Assert.Equal(3, number);
        Assert.False(isDefined);   // (OrderStatus)99 is a legal value of the type
    }

    [Fact]
    public void CastTruncatesAndTryParseDoesNotThrow()
    {
        var (widened, truncated, parsed, tried) = Step01_BuiltInTypes.Conversions();

        Assert.Equal(42L, widened);
        Assert.Equal(3, truncated);   // 3.99 -> 3
        Assert.Equal(42, parsed);
        Assert.False(tried);
    }

    // --- TASK 1 ----------------------------------------------------------------------

    [Theory]
    [InlineData(12, "2026-03-10", true)]     // still inside the warranty
    [InlineData(12, "2026-03-15", true)]     // the last valid day
    [InlineData(12, "2026-03-16", false)]    // one day too late
    public void WarrantyValidity(int months, string todayText, bool expected)
    {
        var purchased = new DateOnly(2025, 3, 15);
        var today = DateOnly.Parse(todayText, System.Globalization.CultureInfo.InvariantCulture);

        var (expiresOn, isValid) = Warranty(purchased, months, today);

        Assert.Equal(new DateOnly(2026, 3, 15), expiresOn);
        Assert.Equal(expected, isValid);
    }

    [Fact]
    public void WarrantyOfZeroMonthsIsNeverValid()
    {
        var purchased = new DateOnly(2026, 3, 15);

        var (expiresOn, isValid) = Warranty(purchased, 0, purchased);

        Assert.Equal(purchased, expiresOn);
        Assert.False(isValid);
    }

    [Fact]
    public void WarrantyUsesCalendarMonths()
    {
        // 31 January + 1 month is 28 February, not 3 March.
        var (expiresOn, _) = Warranty(new DateOnly(2026, 1, 31), 1, new DateOnly(2026, 2, 1));

        Assert.Equal(new DateOnly(2026, 2, 28), expiresOn);
    }

    // --- TASK 2 ----------------------------------------------------------------------

    [Theory]
    [InlineData("2026-03-18T14:45:00Z", "3 days 4 hours")]
    [InlineData("2026-03-15T14:45:00Z", "4 hours 15 minutes")]
    [InlineData("2026-03-15T10:45:00Z", "15 minutes")]
    [InlineData("2026-03-15T09:00:00Z", "not yet")]
    public void OrderAgeIsFormattedWithTheLargestTwoUnits(string nowText, string expected)
    {
        var placed = new DateTimeOffset(2026, 3, 15, 10, 30, 0, TimeSpan.Zero);
        var now = DateTimeOffset.Parse(nowText, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(expected, OrderAge(placed, now));
    }

    [Fact]
    public void OrderAgeIsCorrectAcrossTimeZones()
    {
        // Same instant, written in two different offsets.
        var placed = new DateTimeOffset(2026, 3, 15, 11, 30, 0, TimeSpan.FromHours(1));
        var now = new DateTimeOffset(2026, 3, 15, 10, 45, 0, TimeSpan.Zero);

        Assert.Equal("15 minutes", OrderAge(placed, now));
    }
}
