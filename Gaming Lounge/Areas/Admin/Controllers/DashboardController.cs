using Gaming_Lounge.Data;
using Gaming_Lounge.Models.Enums;
using Gaming_Lounge.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gaming_Lounge.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var statusCounts = await _context.Reservations
            .AsNoTracking()
            .GroupBy(reservation => reservation.Status)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Key, item => item.Count);

        var upcomingQuery = _context.Reservations.Where(reservation =>
            reservation.Status == ReservationStatus.Confirmed
            && (reservation.ReservationDate > today
                || (reservation.ReservationDate == today && reservation.StartTime > now)));

        var customerRoleId = await _context.Roles
            .Where(role => role.Name == "Customer")
            .Select(role => role.Id)
            .SingleOrDefaultAsync();

        var registeredCustomers = customerRoleId is null
            ? 0
            : await _context.UserRoles.CountAsync(userRole => userRole.RoleId == customerRoleId);

        var recentReservations = await _context.Reservations
            .AsNoTracking()
            .OrderByDescending(reservation => reservation.CreatedAtUtc)
            .Take(6)
            .Select(reservation => new AdminReservationRowViewModel
            {
                Id = reservation.Id,
                CustomerName = reservation.User.DisplayName,
                CustomerEmail = reservation.User.Email ?? string.Empty,
                StationName = reservation.GamingStation.Name,
                ReservationDate = reservation.ReservationDate,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status,
                CreatedAtUtc = reservation.CreatedAtUtc
            })
            .ToListAsync();

        return View(new AdminDashboardViewModel
        {
            TotalStations = await _context.GamingStations.CountAsync(),
            AvailableStations = await _context.GamingStations.CountAsync(
                station => station.Status == StationStatus.Available),
            MaintenanceStations = await _context.GamingStations.CountAsync(
                station => station.Status == StationStatus.Maintenance),
            TotalReservations = statusCounts.Values.Sum(),
            TodayReservations = await _context.Reservations.CountAsync(
                reservation => reservation.ReservationDate == today),
            UpcomingReservations = await upcomingQuery.CountAsync(),
            ConfirmedReservations = statusCounts.GetValueOrDefault(ReservationStatus.Confirmed),
            CancelledReservations = statusCounts.GetValueOrDefault(ReservationStatus.Cancelled),
            RegisteredCustomers = registeredCustomers,
            UpcomingRevenue = await upcomingQuery.SumAsync(reservation => (decimal?)reservation.TotalPrice) ?? 0,
            StatusCounts = statusCounts,
            RecentReservations = recentReservations
        });
    }
}