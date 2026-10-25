using Gaming_Lounge.Data;
using Gaming_Lounge.Models.Enums;
using Gaming_Lounge.Services;
using Gaming_Lounge.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gaming_Lounge.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReservationsController(
    ApplicationDbContext context,
    IReservationService reservationService) : Controller
{
    private const int PageSize = 15;
    private readonly ApplicationDbContext _context = context;
    private readonly IReservationService _reservationService = reservationService;

    public async Task<IActionResult> Index(
        ReservationStatus? status,
        string? searchTerm,
        DateOnly? date,
        int page = 1)
    {
        var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
        var query = _context.Reservations.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(reservation => reservation.Status == status.Value);
        }

        if (date.HasValue)
        {
            query = query.Where(reservation => reservation.ReservationDate == date.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(reservation =>
                reservation.User.DisplayName.Contains(normalizedSearch)
                || (reservation.User.Email != null && reservation.User.Email.Contains(normalizedSearch))
                || reservation.GamingStation.Name.Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

        var reservations = await query
            .OrderByDescending(reservation => reservation.ReservationDate)
            .ThenByDescending(reservation => reservation.StartTime)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
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

        return View(new AdminReservationsIndexViewModel
        {
            SearchTerm = normalizedSearch,
            Status = status,
            Date = date,
            Page = page,
            PageSize = PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Reservations = reservations
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        ReservationStatus status,
        ReservationStatus? filterStatus,
        string? searchTerm,
        DateOnly? date,
        int page = 1)
    {
        if (status == ReservationStatus.Cancelled)
        {
            var result = await _reservationService.CancelAsync(id, string.Empty, isAdmin: true);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
            return RedirectToIndex(filterStatus, searchTerm, date, page);
        }

        var reservation = await _context.Reservations.SingleOrDefaultAsync(item => item.Id == id);
        if (reservation is null)
        {
            return NotFound();
        }

        reservation.Status = status;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Reservation status updated.";
        return RedirectToIndex(filterStatus, searchTerm, date, page);
    }

    private IActionResult RedirectToIndex(
        ReservationStatus? status,
        string? searchTerm,
        DateOnly? date,
        int page) => RedirectToAction(nameof(Index), new
        {
            status,
            searchTerm,
            date = date?.ToString("yyyy-MM-dd"),
            page
        });
}