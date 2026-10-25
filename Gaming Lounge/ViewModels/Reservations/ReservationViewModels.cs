using System.ComponentModel.DataAnnotations;
using Gaming_Lounge.Models.Enums;

namespace Gaming_Lounge.ViewModels.Reservations;

public sealed class ReservationCreateViewModel
{
    [Required]
    public int GamingStationId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly? ReservationDate { get; set; }

    [Required, DataType(DataType.Time)]
    public TimeOnly? StartTime { get; set; }

    [Range(1, 6)]
    public int DurationHours { get; set; } = 1;

    public string StationName { get; set; } = string.Empty;
    public string StationImagePath { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public DateOnly EarliestDate { get; set; }
    public DateOnly LatestDate { get; set; }
}

public sealed class ReservationListItemViewModel
{
    public int Id { get; init; }
    public string StationName { get; init; } = string.Empty;
    public string StationImagePath { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public DateOnly ReservationDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public int DurationHours { get; init; }
    public decimal TotalPrice { get; init; }
    public ReservationStatus Status { get; init; }
    public bool CanCancel { get; init; }
}

public sealed class ReservationHistoryViewModel
{
    public IReadOnlyList<ReservationListItemViewModel> Upcoming { get; init; } = [];
    public IReadOnlyList<ReservationListItemViewModel> Previous { get; init; } = [];
}

public sealed class ReservationConfirmationViewModel
{
    public int Id { get; init; }
    public string StationName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public DateOnly ReservationDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public int DurationHours { get; init; }
    public decimal HourlyRate { get; init; }
    public decimal TotalPrice { get; init; }
}
