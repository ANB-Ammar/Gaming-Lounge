namespace Gaming_Lounge.Services;

public static class ReservationRules
{
    public static bool Overlaps(
        TimeOnly firstStart,
        TimeOnly firstEnd,
        TimeOnly secondStart,
        TimeOnly secondEnd) => firstStart < secondEnd && secondStart < firstEnd;

    public static bool IsWithinOpeningHours(
        TimeOnly start,
        TimeOnly end,
        TimeOnly open,
        TimeOnly close) => start >= open && end <= close && end > start;
}
