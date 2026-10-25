using System.ComponentModel.DataAnnotations;
using Gaming_Lounge.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gaming_Lounge.ViewModels.Admin;

public sealed class AdminDashboardViewModel
{
    public int TotalStations { get; init; }
    public int AvailableStations { get; init; }
    public int MaintenanceStations { get; init; }
    public int TotalReservations { get; init; }
    public int TodayReservations { get; init; }
    public int UpcomingReservations { get; init; }
    public int ConfirmedReservations { get; init; }
    public int CancelledReservations { get; init; }
    public int RegisteredCustomers { get; init; }
    public decimal UpcomingRevenue { get; init; }
    public IReadOnlyDictionary<ReservationStatus, int> StatusCounts { get; init; } =
        new Dictionary<ReservationStatus, int>();
    public IReadOnlyList<AdminReservationRowViewModel> RecentReservations { get; init; } = [];
}

public sealed class AdminStationsIndexViewModel
{
    public string SearchTerm { get; init; } = string.Empty;
    public int? CategoryId { get; init; }
    public StationStatus? Status { get; init; }
    public IReadOnlyList<SelectListItem> Categories { get; init; } = [];
    public IReadOnlyList<AdminStationRowViewModel> Stations { get; init; } = [];
}

public sealed class AdminStationRowViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public decimal HourlyRate { get; init; }
    public StationStatus Status { get; init; }
    public bool IsFeatured { get; init; }
    public int ReservationCount { get; init; }
}

public sealed class AdminReservationsIndexViewModel
{
    public string SearchTerm { get; init; } = string.Empty;
    public ReservationStatus? Status { get; init; }
    public DateOnly? Date { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<AdminReservationRowViewModel> Reservations { get; init; } = [];
}

public sealed class AdminReservationRowViewModel
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string StationName { get; init; } = string.Empty;
    public DateOnly ReservationDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public decimal TotalPrice { get; init; }
    public ReservationStatus Status { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class AdminCustomersIndexViewModel
{
    public string SearchTerm { get; init; } = string.Empty;
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<AdminCustomerViewModel> Customers { get; init; } = [];
}

public sealed class AdminStationEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(800)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Specifications { get; set; } = string.Empty;

    [Range(1, 10000)]
    [Display(Name = "Hourly rate")]
    public decimal HourlyRate { get; set; }

    [Required, StringLength(260)]
    [Display(Name = "Image path")]
    public string ImagePath { get; set; } = "/images/stations/pc-arena.svg";

    [Display(Name = "Category")]
    public int GamingCategoryId { get; set; }
    public StationStatus Status { get; set; } = StationStatus.Available;
    public bool IsFeatured { get; set; }
    public string? RowVersion { get; set; }
    public IReadOnlyList<SelectListItem> Categories { get; set; } = [];
}

public sealed class AdminCategoryEditViewModel
{
    public int Id { get; set; }
    [Required, StringLength(60)] public string Name { get; set; } = string.Empty;
    [StringLength(80)] public string Slug { get; set; } = string.Empty;
    [Required, StringLength(300)] public string Description { get; set; } = string.Empty;
    [Required, StringLength(40)] public string IconClass { get; set; } = "bi-controller";
    [Required, StringLength(20)] public string AccentColor { get; set; } = "cyan";
    public bool IsActive { get; set; } = true;
    [Range(0, 100)] public int SortOrder { get; set; }
}

public sealed class AdminCustomerViewModel
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public int ReservationCount { get; init; }
}