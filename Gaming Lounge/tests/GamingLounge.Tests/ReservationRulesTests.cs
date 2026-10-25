using Gaming_Lounge.Services;

namespace GamingLounge.Tests;

public class ReservationRulesTests
{
    [Fact]
    public void Overlaps_WhenRequestedTimeIntersectsExisting_ReturnsTrue()
    {
        var result = ReservationRules.Overlaps(
            new TimeOnly(18, 0),
            new TimeOnly(20, 0),
            new TimeOnly(19, 0),
            new TimeOnly(21, 0));

        Assert.True(result);
    }

    [Fact]
    public void Overlaps_WhenRequestedTimeIsInsideExisting_ReturnsTrue()
    {
        var result = ReservationRules.Overlaps(
            new TimeOnly(18, 0),
            new TimeOnly(22, 0),
            new TimeOnly(19, 0),
            new TimeOnly(20, 0));

        Assert.True(result);
    }

    [Fact]
    public void Overlaps_WhenSessionsTouchButDoNotIntersect_ReturnsFalse()
    {
        var result = ReservationRules.Overlaps(
            new TimeOnly(18, 0),
            new TimeOnly(20, 0),
            new TimeOnly(20, 0),
            new TimeOnly(22, 0));

        Assert.False(result);
    }

    [Theory]
    [InlineData(10, 12, true)]
    [InlineData(9, 12, false)]
    [InlineData(21, 23, true)]
    [InlineData(22, 0, false)]
    public void IsWithinOpeningHours_ValidatesBoundaries(int startHour, int endHour, bool expected)
    {
        var result = ReservationRules.IsWithinOpeningHours(
            new TimeOnly(startHour, 0),
            new TimeOnly(endHour, 0),
            new TimeOnly(10, 0),
            new TimeOnly(23, 0));

        Assert.Equal(expected, result);
    }
}
