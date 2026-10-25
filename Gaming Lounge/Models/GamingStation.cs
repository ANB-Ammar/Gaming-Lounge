using System.ComponentModel.DataAnnotations;
using Gaming_Lounge.Models.Enums;

namespace Gaming_Lounge.Models;

public class GamingStation
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(800)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Specifications { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public decimal HourlyRate { get; set; }

    [Required, StringLength(260)]
    public string ImagePath { get; set; } = string.Empty;

    public StationStatus Status { get; set; } = StationStatus.Available;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int GamingCategoryId { get; set; }
    public GamingCategory GamingCategory { get; set; } = null!;

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public ICollection<Reservation> Reservations { get; set; } = [];
}
