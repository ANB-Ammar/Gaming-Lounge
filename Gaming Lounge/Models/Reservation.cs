using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gaming_Lounge.Models.Enums;

namespace Gaming_Lounge.Models;

public class Reservation
{
    public int Id { get; set; }
    public DateOnly ReservationDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [Range(1, 12)]
    public int DurationHours { get; set; }

    public decimal HourlyRate { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAtUtc { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int GamingStationId { get; set; }
    public GamingStation GamingStation { get; set; } = null!;

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    [NotMapped]
    public DateTime StartsAt => ReservationDate.ToDateTime(StartTime);
}
