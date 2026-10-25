namespace Gaming_Lounge.Options;

public sealed class LoungeOptions
{
    public const string SectionName = "Lounge";

    public TimeSpan OpenTime { get; set; } = new(10, 0, 0);
    public TimeSpan CloseTime { get; set; } = new(23, 0, 0);
    public int MinimumDurationHours { get; set; } = 1;
    public int MaximumDurationHours { get; set; } = 6;
    public int AdvanceBookingDays { get; set; } = 30;
    public int CancellationCutoffHours { get; set; } = 2;
    public string TimeZoneId { get; set; } = "Africa/Cairo";
}
